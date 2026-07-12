@base @shared @fileprofiles
Feature: FileProfile

Background:
	Given I create the following api scopes
	| Name | DisplayName | Description |
	| fileProcessor | File Processor REST Scope | A scope for File Processor REST |

	Given the following api resources exist
	| Name | DisplayName | Secret | Scopes | UserClaims |
	| fileProcessor | File Processor REST | Secret1 | fileProcessor | |

	Given the following clients exist
	| ClientId | ClientName | Secret | Scopes | GrantTypes |
	| serviceClient | Service Client | Secret1 | fileProcessor | client_credentials |

	Given I have a token to access the estate management and transaction processor resources
	| ClientId |
	| serviceClient |

Scenario: Create, update and list file profiles
	When I create the following file profiles
	| Alias | FileProfileId | Name | ListeningDirectory | RequestType | OperatorName | LineTerminator | FileFormatHandler |
	| Profile A | 11111111-1111-1111-1111-111111111111 | Profile A | /home/txnproc/bulkfiles/profile-a | ProfileARequest | Operator A | \n | ProfileAHandler |
	| Profile B | 22222222-2222-2222-2222-222222222222 | Profile B | /home/txnproc/bulkfiles/profile-b | ProfileBRequest | Operator B | \r\n | ProfileBHandler |
	And I update the following file profiles
	| Alias | Name | ListeningDirectory | RequestType | OperatorName | LineTerminator | FileFormatHandler |
	| Profile A | Profile A Updated | /home/txnproc/bulkfiles/profile-a-updated | ProfileARequestUpdated | Operator A Updated | \r\n | ProfileAHandlerUpdated |
	Then the file profiles list should contain 2 items
	And the file profiles should have the following values
	| Alias | Name | ListeningDirectory | RequestType | OperatorName | LineTerminator | FileFormatHandler |
	| Profile A | Profile A Updated | /home/txnproc/bulkfiles/profile-a-updated | ProfileARequestUpdated | Operator A Updated | \r\n | ProfileAHandlerUpdated |
	| Profile B | Profile B | /home/txnproc/bulkfiles/profile-b | ProfileBRequest | Operator B | \r\n | ProfileBHandler |

Scenario: Reject duplicate file profile name and request type
	When I create the following file profiles
	| Alias | FileProfileId | Name | ListeningDirectory | RequestType | OperatorName | LineTerminator | FileFormatHandler |
	| Profile A | 33333333-3333-3333-3333-333333333333 | Profile A | /home/txnproc/bulkfiles/profile-a | ProfileARequest | Operator A | \n | ProfileAHandler |
	| Profile B | 44444444-4444-4444-4444-444444444444 | Profile B | /home/txnproc/bulkfiles/profile-b | ProfileBRequest | Operator B | \r\n | ProfileBHandler |
	And I try to create the following duplicate file profiles
	| DuplicateType | BasedOn |
	| Name | Profile A |
	| RequestType | Profile B |
	And the file profiles list should contain 2 items
