using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Elite.Task.Microservice.Repository.Contracts;
using Elite_Task.Microservice.Application.CQRS.Commands;
using Elite_Task.Microservice.Models.Entities;
using Elite.Task.Microservice.Application.CQRS.Commands.CommandsDto;
using Elite.Task.Microservice.Models.Entities;
using Elite_Task.Microservice.Repository.Contracts;
using Elite.Common.Utilities.Attachment.Delete.MapOrphans;
using Elite.Common.Utilities.CommonType;
using Elite.Common.Utilities.UserRolesAndPermissions;
using Elite.Task.Microservice.Application.CQRS.ExternalService;
using Elite.Common.Utilities.RequestContext;
using Elite.Task.Microservice.CommonLib;
using Elite.Common.Utilities.ExceptionHandling;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Net.Http;
using System.Text;
using Polly.Retry;
using Polly;
using System.Net;
using Microsoft.Extensions.Logging;

namespace Elite.Task.Microservice.Application.CQRS.Commands
{
    public class TaskCommentHandler : BaseCommandHandler<TaskCommentCommand>, IRequestHandler<TaskCommentCommand, long>
    {
        protected readonly IMediator _mediator;
        protected readonly ICommentRepository _repository;
        protected readonly IConfiguration _configuration;
        private readonly IAttachmentService _attachmentService;
        private readonly Func<IConfiguration, IAttachmentService> _attachmentServiceFactory;
        protected readonly ITaskRepository _taskRepository;
        private readonly IUserService _userService;
        private readonly Func<IConfiguration, IRequestContext, IUserService> _userServiceFactory;
        private RolesAndRights<IUserRolesPermissions> rolesPermissions;
        private string pUID;
        private readonly string securedUID = string.Empty;
        private string _templatePath = string.Empty;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _httpRequestPolicy;
        private readonly HttpClient _httpClient;
        private readonly ITaskLog _taskLog;
        private readonly ILogger<TaskCommentHandler> _logger;

        public TaskCommentHandler(IMediator mediator, ICommentRepository repository, IConfiguration configuration, Func<IConfiguration, IAttachmentService> attachmentServiceFactory,
                        ITaskRepository taskrepository, IRequestContext requestContext, Func<IConfiguration, IRequestContext, IUserService> userServiceFactory, ITaskLog taskLog, ILogger<TaskCommentHandler> logger)
        {
            _mediator = mediator;
            _repository = repository;
            _configuration = configuration;
            this._attachmentServiceFactory = attachmentServiceFactory;
            this._attachmentService = this._attachmentServiceFactory(this._configuration);
            _taskRepository = taskrepository;
            _userServiceFactory = userServiceFactory;
            this._userService = _userServiceFactory(configuration, requestContext);
            rolesPermissions = new RolesAndRights<IUserRolesPermissions>(requestContext, _userService);
            this.pUID = requestContext.IsDeputy ? requestContext.DeputyUID != null ? requestContext.DeputyUID.Upper() : string.Empty
                                                : requestContext.UID != null ? requestContext.UID.ToUpper() : string.Empty;
            securedUID = requestContext.DecrpUID;
            _templatePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + _configuration.GetSection("EmailNotification:TaskComment").Value;
            _httpClient = new HttpClient();
            _httpRequestPolicy = Policy.HandleResult<HttpResponseMessage>(
                 r => r.StatusCode == HttpStatusCode.InternalServerError)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt));
            _taskLog = taskLog;
            _logger = logger;
        }


        public async Task<long> Handle(TaskCommentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                
                return await SaveandUpdateComments(request);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<long> SaveandUpdateComments(TaskCommentCommand request)
        {
            if (request.TaskId.HasValue)
            {
                var task = await _taskRepository.GetByIdAsync(request.TaskId.Value);
                CheckRoles(task);
                if(task.ParentId is null)
                   await PrepareMail(task, request.Comment);
            }

            TaskComment comment = new TaskComment();
            List<string> delAttachments = new List<string>();
            comment.Comment = request.Comment;
            comment.TaskId = request.TaskId;
            comment.Id = request.Id;
            comment.CreatedBy = JsonConvert.SerializeObject(_userService.GetUserDetail(securedUID));
            comment.CreatedDate = DateTime.Now;
            comment.ModifiedBy = JsonConvert.SerializeObject(_userService.GetUserDetail(securedUID));
            comment.ModifiedDate = DateTime.Now;
            CreateCommentAttachmentMapping(comment, comment.Id, request.Attachments, JsonConvert.DeserializeObject<TaskPersonCommand>(comment.CreatedBy), delAttachments);

            try
            {
                if (IsUpdate(comment.Id))
                    UpdateComment(comment, delAttachments);
                else
                    Create(comment);
                await _repository.UnitOfWork.SaveEntitiesAsync();

                //publishing to kafka ---> to delete the attachment from Attachment service
                PublishThroughEventBusForDeleteAttachments(AttachmentHelper.GetAttachments(request.Attachments, AttachmnetType.DeleteAttachment));

                //publishing to kafka ---> to set IsOrphen to false for mapping topic attachment
                PublishThroughEventBusForMappingAttachments(AttachmentHelper.GetAttachments(request.Attachments, AttachmnetType.MappOrphanAttachment));

                
            }
            catch (Exception ex)
            {
                throw;
            }
            return comment.Id;
        }

        public async System.Threading.Tasks.Task PrepareMail(EliteTask eliteTask, string comment)
        {
            try
            {
                await System.Threading.Tasks.Task.Run(async () => {
                    //get committeename
                    var committeeConfigSettings = await _userService.GetCommitteeConfigSettingsAsync(eliteTask.CommitteeId ?? 0);

                    if (committeeConfigSettings.IsSB)
                    {
                        var committees = await _userService.GetCommitees();
                        var committee = committees.Where(w => w.CommitteeId == eliteTask.CommitteeId).FirstOrDefault();
                        var committeeManagers = await _userService.GetCommitteeManagersAsync(eliteTask.CommitteeId ?? 0);
                        var committeeMangersEmailId = committeeManagers.Select(s => s.EmailId).ToList();

                        List<SendEmail> emailList = new List<SendEmail>();
                        SendEmail sendEmail = new SendEmail();
                        string body = string.Empty;
                        List<string> emailIds = new List<string>();

                        if (File.Exists(_templatePath))
                        {
                            body = File.ReadAllText(_templatePath);
                        }

                        if (!string.IsNullOrEmpty(body))
                        {
                            body = body.Replace("#TASKTITLE#", eliteTask.Title);
                            body = body.Replace("#RESPONSIBLE#", JsonConvert.DeserializeObject<TaskPersonCommand>(eliteTask.Responsible).FullName);
                            body = body.Replace("#TASKCOMMENT#", comment);
                            body = body.Replace("#DUEDATE#", eliteTask.DueDate.HasValue ? eliteTask.DueDate.Value.ToString("dd.MM.yyyy") : "");
                            body = body.Replace("#TASKHEADER#", GetTaskHeader());
                            body = body.Replace("#TASKACTIONLINK#", $"{_configuration.GetSection("EmailNotification:TaskActionLink").Value}?id={eliteTask.Id}");
                        }

                        emailIds.AddRange(committeeMangersEmailId);
                        sendEmail.Receipients = emailIds;
                        sendEmail.Subject = $"{committee.CommitteeName} - eLite Task comment";
                        sendEmail.Body = body;
                        sendEmail.basePath = _configuration.GetSection("BasePathTemplate").Value;
                        sendEmail.SMTPEmailwithTemplate = SendEmailType.TASK;
                        emailList.Add(sendEmail);

                        if (string.IsNullOrEmpty(committeeConfigSettings.PoolIdEmailId))
                            await PostMail(emailList);
                        else
                            await PostMail(emailList, committeeConfigSettings);
                    }
                });
            }

            catch (Exception ex)
            {
                _taskLog.LogTaskError(ex);
            }
            
        }

        private string GetTaskHeader()
        {
            StringBuilder headerContent = new StringBuilder();
            headerContent.Append("<td style='background: #000; padding: 0px 5em; padding-bottom: 3px; text-align:center; width:100%'>")
                    .Append("<span style='color: #c0c0c0; font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif; font-size: 11px;'>")
                    .Append("***This is an automatically generated e-mail by eLite application, please do not reply to this e-mail*** </span></td>");

            return headerContent.ToString();
        }

        private async System.Threading.Tasks.Task PostMail(List<SendEmail> sendEmails)
        {
            try
            {
                var outLookRequest = JsonConvert.SerializeObject(sendEmails);
                var stringContent = new StringContent(outLookRequest, UnicodeEncoding.UTF8, "application/json");
                await _httpRequestPolicy.ExecuteAsync(async () => await _httpClient.PostAsync(_configuration.GetSection("EmailNotification:outlookapi").Value + "/SMTPEmail/", stringContent));
            }

            catch(Exception ex)
            {
                _logger.LogError($" {nameof(TaskCommentHandler)}  SendEmail failed   ");
                _logger.LogError(ExceptionFormator.FormatExceptionMessage(ex));
            }
        }

        private async System.Threading.Tasks.Task PostMail(List<SendEmail> sendEmails, Helpers.CommitteeConfigSettings committeeConfigSettings)
        {
            try
            {
                SendTaskEmailPoolId sendTaskEmailPoolId = new SendTaskEmailPoolId();
                sendTaskEmailPoolId.PoolIdEmailId = committeeConfigSettings.PoolIdEmailId;
                sendTaskEmailPoolId.PoolIdName = committeeConfigSettings.PoolIdName;
                sendTaskEmailPoolId.SMPTPMailList = sendEmails;

                var outLookRequest = JsonConvert.SerializeObject(sendTaskEmailPoolId);
                var stringContent = new StringContent(outLookRequest, UnicodeEncoding.UTF8, "application/json");
                await _httpRequestPolicy.ExecuteAsync(async () => await _httpClient.PostAsync(_configuration.GetSection("EmailNotification:outlookapi").Value + "/SMTPPoolIdEmail/", stringContent));
            }

            catch (Exception ex)
            {
                _logger.LogError($" {nameof(TaskCommentHandler)}  SendEmail failed   ");
                _logger.LogError(ExceptionFormator.FormatExceptionMessage(ex));
            }
        }

        private void CreateCommentAttachmentMapping(TaskComment comment, long taskId, IList<TaskCommentAttachmentDto> attachments, TaskPersonCommand user, List<string> delAttachments)
        {
            if (attachments?.Count > 0)
            {
                foreach (var attachment in attachments)
                {
                    if (attachment.IsDeleted == false)
                    {
                        comment.TaskCommentAttachmentMapping.Add(AttachmentMapperAsync(comment, attachment, user));
                    }
                    else
                        delAttachments.Add(attachment.AttachmentGuid);
                }
            }
        }

        private TaskCommentAttachmentMapping AttachmentMapperAsync(TaskComment comment, TaskCommentAttachmentDto file, TaskPersonCommand user)
        {
            TaskCommentAttachmentMapping commentAttachment = new TaskCommentAttachmentMapping();
            commentAttachment.AttachmentName = file.AttachmentDesc;
            commentAttachment.AttachmentGuid = file.AttachmentGuid;
            commentAttachment.AttachmentSize = file.AttachmentSize;
            commentAttachment.CreatedBy = JsonConvert.SerializeObject(user);
            commentAttachment.CreatedDate = System.DateTime.Now;
            commentAttachment.Comment = comment;
            return commentAttachment;
        }

        private void Create(TaskComment request)
        {
            _repository.Add(request);
        }


        private void UpdateComment(TaskComment request, List<string> delAttachments)
        {
            if (request != null)
            {
                if (request.TaskCommentAttachmentMapping.Count > 0)
                    _repository.Add(request);

                _repository.Update(request);

                if (delAttachments.Count > 0)
                    _repository.Delete(delAttachments);
            }
            else
                throw new NullReferenceException($" Comment was null");
        }

        private void PublishThroughEventBusForDeleteAttachments(IList<TaskCommentAttachmentDto> attachments)
        {
            if (attachments?.Count > 0)
            {
                var evt = new AttachmentDeleteOrMappingEvent();
                attachments.ToList().ForEach(p => evt.AttachmentGuids.Add(p.AttachmentGuid));
                this._attachmentService.PublishThroughEventBusForDelete(evt);
            }
        }

        private void PublishThroughEventBusForMappingAttachments(IList<TaskCommentAttachmentDto> attachments)
        {
            if (attachments?.Count > 0)
            {
                var evt = new AttachmentDeleteOrMappingEvent();
                attachments.ToList().ForEach(p => evt.AttachmentGuids.Add(p.AttachmentGuid));
                this._attachmentService.PublishThroughEventBusForMapping(evt);
            }
        }

        private void CheckRoles(EliteTask task)
        {
            if (this.rolesPermissions.UserRolesAndRights?.Count > 0)
            {
                RolePermissions rolePermissions = new RolePermissions();

                var roles = task.CommitteeId.HasValue ? this.rolesPermissions.UserRolesAndRights.SingleOrDefault(s => s.CommitteeId.Equals(task.CommitteeId)) : null;
                var roleId = roles != null ? roles.RoleId : (int?)null;
                var substask = task.InverseParent;

                var permissionsActions = rolePermissions.GetUserAction(_configuration, task.CreatedBy.Upper(), task.Responsible.Upper(), task.CoResponsibles != null ? task.CoResponsibles.Upper() : "", substask.Any(s => s.Responsible.Upper().Contains(pUID)), roleId.HasValue ? roleId : (this.rolesPermissions.IsCmCoMUser) ? (int?)null : (int)RolesType.Transient, task.MeetingId.HasValue, pUID);

                if (!rolePermissions.ValidateActionType(permissionsActions.ToList(), TaskEntityActionType.Comment))
                {
                    throw new EliteException($"unauthorized");
                }
            }
            else { throw new EliteException($"unauthorized"); }
        }



    }
}
