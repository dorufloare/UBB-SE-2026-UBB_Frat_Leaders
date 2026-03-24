classDiagram
direction TB




    %% ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    %% ENUMS
    %% ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    namespace Enums {
        class MatchStatus {
            <<enumeration>>
            Applied
            Accepted
            Rejected
        }
        class AppMode {
            <<enumeration>>
            UserMode
            CompanyMode
        }
        class MessageType {
            <<enumeration>>
            Text
            Image
            File
        }
    }












    %% ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    %% ENTITIES / MODELS
    %% ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    namespace Entities {
        class User {
            +int UserId
            +string Name
            +string Location
            +string Email
            +string Phone
            +int YearsOfExperience
            +string Education
            +string Resume
            +string PreferredEmploymentType
        }
        class Skill {
            +int UserId
            +int SkillId
            +string SkillName
            +int Score
        }
        class Job {
            +int JobId
            +string JobDescription
            +string Location
            +string EmploymentType
            +int CompanyId
            +int PromotionLevel
        }
        class Company {
            +int CompanyId
            +string CompanyName
            +string Email
            +string Phone
        }
        class JobSkill {
            +int JobId
            +int SkillId
            +string SkillName
            +int Score
        }
        class Chat {
            +int ChatId
            +int UserId
            +int? CompanyId
        +int? SecondUserId
            +int? JobId
            +bool isBlocked
            +int? BlockedByUserId
            +bool isDeletedByUser
            +bool isDeletedBySecondParty
        }
        class Message {
            +int MessageId
            +string Content
            +int SenderId
            +DateTime Timestamp
            +int ChatId
            +MessageType Type
            +bool isRead
        }
        class Match {
            +int MatchId
            +int UserId
            +int JobId
            +MatchStatus Status
            +DateTime Timestamp
            +string FeedbackMessage
        }
        class Recommendation {
            +int RecommendationId
            +int UserId
            +int JobId
            +DateTime Timestamp
        }
        class Developer {
            +int DeveloperId
            +string Name
            +string Password
        }
        class Post {
            +int PostId
            +int DeveloperId
            +string Parameter
            +string Value
        }
        class Interaction {
            +int InteractionId
            +int DeveloperId
            +int PostId
            +bool Type
        }
    }
















    %% ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    %% DTOs / RESULT MODELS
    %% ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    namespace DTOs {
        class JobRecommendationResult {
            +Job Job
            +Company Company
            +double CompatibilityScore
            +List~JobSkill~ RequiredSkills
        }
        class UserApplicationResult {
            +User User
            +Match Match
            +Job Job
            +double CompatibilityScore
            +List~Skill~ UserSkills
        }
       class SkillGapEntry {
           +string SkillName
           +int UserScore
           +int RequiredScore
           +int JobCount
       }
        class TestResult {
            +int MatchId
            +int UserId
            +int JobId
            +MatchStatus Decision
            +string FeedbackMessage
            +bool IsValid
            +List~string~ ValidationErrors
       }






    }
















    %% ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    %% REPOSITORY IMPLEMENTATIONS
    %% ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    namespace RepositoryImplementations {
        class SqlUserRepository {
            -string _connectionString
            +GetById(int) User
            +GetAll() List~User~
            +Add(User) void
            +Remove(int) void
            +Update(User) void
        }
        class SqlSkillRepository {
            -string _connectionString
            +GetById(int, int) Skill
            +GetAll() List~Skill~
            +Add(Skill) void
            +Remove(int, int) void
            +Update(Skill) void
        }
        class SqlJobRepository {
            -string _connectionString
            +GetById(int) Job
            +GetAll() List~Job~
            +Add(Job) void
            +Remove(int) void
            +Update(Job) void
        }
        class SqlCompanyRepository {
            -string _connectionString
            +GetById(int) Company
            +GetAll() List~Company~
            +Add(Company) void
            +Remove(int) void
            +Update(Company) void
        }
        class SqlDeveloperRepository {
            -string _connectionString
            +GetById(int) Developer
            +GetAll() List~Developer~
            +Add(Developer) void
            +Remove(int) void
            +Update(Developer) void
        }
        class SqlPostRepository {
            -string _connectionString
            +GetById(int) Post
            +GetAll() List~Post~
            +GetByDeveloperId(int developerId) List~Post~
            +Add(Post) void
            +Remove(int) void
            +Update(Post) void
        }
        class SqlInteractionRepository {
            -string _connectionString
            +GetById(int) Interaction
            +GetAll() List~Interaction~
            +GetByDeveloperId(int developerId) List~Interaction~
            +GetByPostId(int postId) List~Interaction~
            +Add(Interaction) void
            +Remove(int) void
            +Update(Interaction) void
        }
        class SqlJobSkillRepository {
            -string _connectionString
            +GetById(int, int) JobSkill
            +GetAll() List~JobSkill~
            +Add(JobSkill) void
            +Remove(int, int) void
            +Update(JobSkill) void
        }
        class SqlChatRepository {
            -string _connectionString
            +GetByUserId(int) List~Chat~
         +GetByCompanyId(int companyId) List~Chat~
         +GetByUsers(int, int) Chat
            +GetByUserAndCompany(int, int) Chat
            +Add(Chat) void
            +BlockChat(int chatId, int blockerId) void
            +UnblockUser(int chatId, int requesterId) void
            +DeletedByUser(int chatId, int userId) void
            +DeletedBySecondParty(int chatId, int callerId) void
        }
        class SqlMessageRepository {
            -string _connectionString
            +GetByChatId(int) List~Message~
            +Add(Message) void
            +MarkAsRead(int chatId, int readerId) void
        }
        class SqlRecommendationRepository {
            -string _connectionString
            +GetById(int) Recommendation
            +GetAll() List~Recommendation~
            +Add(Recommendation) void
            +Remove(int) void
            +Update(Recommendation) void
            -Load() void
            -Save() void
        }
        class SqlMatchRepository {
            -string _connectionString
            +GetById(int) Match
            +GetAll() List~Match~
            +Add(Match) void
            +Remove(int) void
            +Update(Match)
            -Load() void
            -Save() void
        }
    }
















    %% ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    %% SERVICE IMPLEMENTATIONS
    %% ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    namespace ServiceImplementations {
        class SessionService {
            -int _currentUserId
            -int _currentCompanyId
            -AppMode _currentMode
            +int CurrentUserId
            +int CurrentCompanyId
            +AppMode CurrentMode
            +SwitchToUserMode() void
            +SwitchToCompanyMode() void
        }
        class SkillService {
            +GetByUserId(int userId) List~Skill~
        }
        class JobService {
            +GetByCompanyId(int companyId) List~Job~
        }
        class JobSkillService {
            +GetByJobId(int jobId) List~JobSkill~
        }
        class RecommendationService {
            +GetByUserId(int userId) List~Recommendation~
            +GetByJobId(int jobId) List~Recommendation~
        }
        class DeveloperService {
            +GetById(int developerId) Developer
            +GetAll() List~Developer~
        }
        class PostService {
            +GetByDeveloperId(int developerId) List~Post~
            +GetById(int postId) Post
        }
        class InteractionService {
            +GetByDeveloperId(int developerId) List~Interaction~
            +GetByPostId(int postId) List~Interaction~
        }
        class RecommendationAlgorithm {
            +CalculateCompatibilityScore(User, Job, List~Skill~, List~JobSkill~, double, double, double, double) double
            +CalculatePreferenceScore(User, Job) double
            -CalculateSkillScore(List~Skill~, List~JobSkill~) double
            -CalculateKeywordScore(string, string) double
            -CalculateLocationScore(string, string) double
            -CalculateEmploymentTypeScore(string, string) double
            -CalculatePromotionScore(Job) double
        }
        class UserRecommendationService {
            +GetNextJobForUser(int userId) JobRecommendationResult
            -GetJobsByCompatibility(int userId) List~JobRecommendationResult~
        }
        class CompanyRecommendationService {
            +GetNextApplicantForCompany(int companyId) UserApplicationResult
            -GetApplicantsByCompatibility(int companyId) List~UserApplicationResult~
        }
        class MatchService {
            +Apply(int userId, int jobId) void
            +Accept(int matchId) void
            +Reject(int matchId) void
            +GetMatchesForUser(int userId) List~Match~
            +GetMatchesForCompany(int companyId) List~Match~
        }
        class ChatService {
            +GetChatsForUser(int userId) List~Chat~
            +GetChatsForCompany(int companyId) List~Chat~
            +GetMessages(int chatId) List~Message~
            +SendMessage(int chatId, string content, int senderId, MessageType type)
            +FindOrCreateUserCompanyChat(int userId, int companyId, int? jobId) Chat
        +FindOrCreateUserUserChat(int userId, int secondUserId) Chat
            +SearchCompanies(string query) List~Company~
            +SearchUsers(string query) List~User~
            +BlockUser(int chatId, int blockerId) void
            +UnblockUser(int chatId, int requesterId) void
            +DeleteChat(int chatId, int callerId) void
            +MarkMessageAsRead(int chatId, int readerId) void
        }
         class CooldownService {
            -int _cooldownMinutes
            +RecordShown(int userId, int jobId) void
            +IsOnCooldown(int userId, int jobId) bool
        }




    }
















   
   




    CooldownService --> SqlRecommendationRepository








    UserRecommendationService --> CooldownService
    UserRecommendationService --> SqlJobRepository
    UserRecommendationService --> SqlJobSkillRepository
    UserRecommendationService --> SqlSkillRepository
    UserRecommendationService --> SqlCompanyRepository
    UserRecommendationService --> SqlRecommendationRepository
    UserRecommendationService --> RecommendationAlgorithm












    CompanyRecommendationService --> CooldownService
    CompanyRecommendationService --> SqlMatchRepository
    CompanyRecommendationService --> SqlUserRepository
    CompanyRecommendationService --> SqlSkillRepository
    CompanyRecommendationService --> SqlJobRepository
    CompanyRecommendationService --> SqlJobSkillRepository
    CompanyRecommendationService --> SqlRecommendationRepository
    CompanyRecommendationService --> RecommendationAlgorithm
















    MatchService --> SqlMatchRepository
    MatchService --> SqlJobRepository
















    ChatService --> SqlChatRepository
    ChatService --> SqlMessageRepository
    ChatService --> SqlCompanyRepository
    ChatService --> SqlUserRepository

    DeveloperService --> SqlDeveloperRepository
    PostService --> SqlPostRepository
    InteractionService --> SqlInteractionRepository












    SkillService --> SqlSkillRepository
    JobService --> SqlJobRepository
    JobSkillService --> SqlJobSkillRepository
    RecommendationService --> SqlRecommendationRepository

    Developer --> Post
    Developer --> Interaction
    Post --> Interaction




    UserRecommendationService --> JobService
    UserRecommendationService --> JobSkillService
    UserRecommendationService --> SkillService
    UserRecommendationService --> RecommendationService




    CompanyRecommendationService --> MatchService
    CompanyRecommendationService --> SkillService
    CompanyRecommendationService --> JobService
    CompanyRecommendationService --> JobSkillService
    CompanyRecommendationService --> RecommendationService












    %% ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    %% VIEWMODELS
    %% ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    namespace ViewModels {
        class ObservableObject {
            <<abstract>>
            +PropertyChanged EventHandler
            #OnPropertyChanged(string propertyName) void
        }
        class MainViewModel {
            +AppMode CurrentMode
            +SwitchMode() void
        }
        class UserRecommendationViewModel {
            +JobRecommendationResult CurrentJob
            +double CompatibilityScore
            +bool HasRecommendation
            +string CurrentJobLocation
            +string CurrentJobEmploymentType
            +string CurrentCompanyEmail
            +string CurrentCompanyPhone
            +ObservableCollection~JobSkill~ CurrentJobSkills
            +Apply() void
            +Skip() void
		+Undo() void


        }
        class CompanyRecommendationViewModel {
            +UserApplicationResult CurrentApplicant
            +double CompatibilityScore
            +bool HasApplicant
            +string ApplicantName
            +string ApplicantLocation
            +string ApplicantEmail
            +string ApplicantPhone
            +int ApplicantYearsOfExperience
            +ObservableCollection~Skill~ ApplicantSkills
            +Accept() void
            +Reject() void
		+Undo() void
        }
        class UserStatusViewModel {
            +ObservableCollection~JobRecommendationResult~ AppliedJobs
            +LoadMatches() void
            +Refresh() void
        }
        class CompanyStatusViewModel {
            +UserApplicationResult SelectedApplicant
            +Match SelectedMatch
            +MatchStatus SelectedDecision
            +string FeedbackMessage
            +bool IsContactUnmasked
            +string MaskedEmail
            +string MaskedPhone
            +string UnmaskedEmail
            +string UnmaskedPhone
            +bool IsLoading
            +string ValidationErrorDecision
            +string ValidationErrorFeedback
            +bool HasValidationErrors
            +TestResult LastTestResult
            +LoadApplicantsAsync() Task
            +LoadEvaluationAsync(int matchId) Task
            +ValidateDecision() bool
            +ValidateFeedback() bool
            +ValidateAll() bool
            +SubmitDecisionAsync() Task
            +UnmaskContactInfo() void
            +CancelEvaluation() void
            +ObservableCollection~UserApplicationResult~ Applications
            +LoadApplications() void
            +Refresh() void
        }
        class SkillGapViewModel {
            +ObservableCollection~SkillGapEntry~ MissingSkills
            +ObservableCollection~SkillGapEntry~ SkillsToImprove
            +string Summary
            +bool HasData
            +LoadData() void
            +Refresh() void
        }




        class ChatViewModel {
            +ObservableCollection~Chat~ Chats
        +ObservableCollection~Chat~ FilteredChats
            +Chat SelectedChat
            +ObservableCollection~Message~ Messages
            +ObservableCollection~object~ ContactSearchResults
            +string MessageText
            +string SearchQuery
        +string ActiveTab
            +MessageType selectedMessageType
            +Job LinkedJob
            +bool ShowBlock
            +bool ShowUnblock
            +bool ShowGoToProfile
            +bool ShowGoToCompanyProfile
            +bool ShowGoToJobPost
            +SendMessage() void
            +SelectChat() void
            +SearchContacts() void
            +StartChat() void
            +BlockUser() void
            +UnblockUser() void
            +DeleteChat() void
            +GoToProfile() void
            +GoToCompanyProfile() void
            +GoToJobPost() void
        +ApplyTabFilter() void
        }
    }
















    MainViewModel <|-- ObservableObject
    CompanyRecommendationViewModel <|-- ObservableObject
    CompanyStatusViewModel <|-- ObservableObject
















    MainViewModel --> SessionService
    UserRecommendationViewModel --> UserRecommendationService
    UserRecommendationViewModel --> MatchService
    UserRecommendationViewModel --> SessionService
    CompanyRecommendationViewModel --> CompanyRecommendationService
    CompanyRecommendationViewModel --> MatchService
    CompanyRecommendationViewModel --> SessionService
    UserStatusViewModel --> MatchService
    UserStatusViewModel --> SessionService
    SkillGapViewModel <|-- ObservableObject
    SkillGapViewModel --> MatchService
    SkillGapViewModel --> SkillService
    SkillGapViewModel --> JobSkillService
    SkillGapViewModel --> SessionService
    CompanyStatusViewModel --> MatchService
    CompanyStatusViewModel --> SessionService
    ChatViewModel --> ChatService
    ChatViewModel --> SessionService
















    %% ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    %% VIEWS
    %% ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    namespace Views {
        class MainWindowView {
            +Frame ContentFrame
            +Navigate(Type page) void
        }
        class UserRecommendationPageView {
            +HandleApplyButtonClick() void
        }
        class CompanyRecommendationPageView {
            +HandleRejectButtonClick() void
            +HandleProfileButtonClick() void
        }
        class UserStatusPageView {
        }
        class CompanyStatusPageView {
            +HandleSubmitDecisionButtonClick() void
            +HandleCancelButtonClick() void
            +HandleUnmaskContactButtonClick() void
            +HandleDecisionDropdownChanged() void
            +HandleFeedbackInputChanged() void
            +HandleApplicantSelectionChanged() void
            +ShowSuccessPopup() void
            +ShowErrorPopup() void
            +ShowValidationFailurePopup() void
            +ShowCancelConfirmationDialog() void
        }
        class SkillGapPageView {
           +HandleRefreshButtonClick() void
        }




        class ChatPageView {
            +HandleSendButtonClick() void
            +HandleChatSelectionChanged() void
            +HandleSearchButtonClick() void
            +HandleStartChatButtonClick() void
            +HandleBlockButtonClick() void
            +HandleUnblockButtonClick() void
            +HandleDeleteChatButtonClick() void
            +HandleAttachmentButtonClick() void
            +HandleGoToProfileButtonClick() void
            +HandleGoToCompanyProfileButtonClick() void
            +HandleGoToJobPostButtonClick() void
        }
    }
















    MainWindowView --> MainViewModel
    UserRecommendationPageView --> UserRecommendationViewModel
    CompanyRecommendationPageView --> CompanyRecommendationViewModel
    UserStatusPageView --> UserStatusViewModel
    SkillGapPageView --> SkillGapViewModel
    CompanyStatusPageView --> CompanyStatusViewModel
    ChatPageView --> ChatViewModel








    %% ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    %% ENTITY RELATIONSHIPS
    Match --> MatchStatus
    User "1" -- "*" Skill : has
    Job "1" -- "*" JobSkill : requires
    Job "*" -- "1" Company : posted by
    Chat "*" -- "1" User : participant
    Chat "*" -- "0..1" User : second participant
    Chat "*" -- "0..1" Company : participant
    Message "*" -- "1" Chat : belongs to
    Match "*" -- "1" User : applicant
    Match "*" -- "1" Job : target
    Recommendation "*" -- "1" User : shown to
    Recommendation "*" -- "1" Job : recommended
    JobRecommendationResult --> Job
    JobRecommendationResult --> Company
    SkillGapViewModel --> SkillGapEntry
    UserApplicationResult --> User
    UserApplicationResult --> Match
    UserApplicationResult --> Job
    CompanyStatusViewModel --> TestResult











