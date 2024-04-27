﻿@base @shared  @getfileimportdetails
Feature: GetFileImportDetails

Background: 
	Given I create the following api scopes
	| Name                 | DisplayName                       | Description                            |
	| estateManagement     | Estate Managememt REST Scope      | A scope for Estate Managememt REST     |
	| transactionProcessor | Transaction Processor REST  Scope | A scope for Transaction Processor REST |
	| voucherManagement | Voucher Management REST  Scope | A scope for Voucher Management REST |
	| fileProcessor | File Processor REST Scope | A scope for File Processor REST |

	Given the following api resources exist
	| Name         | DisplayName                | Secret  | Scopes               | UserClaims                 |
	| estateManagement     | Estate Managememt REST     | Secret1 | estateManagement     | MerchantId, EstateId, role |
	| transactionProcessor | Transaction Processor REST | Secret1 | transactionProcessor |                            |
	| voucherManagement    | Voucher Management REST    | Secret1 | voucherManagement    |                            |
	| fileProcessor        | File Processor REST        | Secret1 | fileProcessor        |                            |

	Given the following clients exist
	| ClientId      | ClientName     | Secret  | Scopes    | GrantTypes  |
	| serviceClient | Service Client | Secret1 | estateManagement,transactionProcessor,voucherManagement,fileProcessor | client_credentials |

	Given I have a token to access the estate management and transaction processor resources
	| ClientId      | 
	| serviceClient | 

	Given I have created the following estates
	| EstateName    |
	| Test Estate 1 |

	Given I have created the following operators
	| EstateName    | OperatorName | RequireCustomMerchantNumber | RequireCustomTerminalNumber |
	| Test Estate 1 | Safaricom    | True                        | True                        |
	| Test Estate 1 | Voucher      | True                        | True                        |

	And I have assigned the following operators to the estates
	| EstateName    | OperatorName |
	| Test Estate 1 | Safaricom    |
	| Test Estate 1 | Voucher      |

	Given I create a contract with the following values
	| EstateName    | OperatorName | ContractDescription |
	| Test Estate 1 | Safaricom    | Safaricom Contract  |
	| Test Estate 1 | Voucher      | Hospital 1 Contract |

	When I create the following Products
	| EstateName    | OperatorName | ContractDescription | ProductName    | DisplayText | Value | ProductType |
	| Test Estate 1 | Safaricom    | Safaricom Contract  | Variable Topup | Custom      |       | MobileTopup |
	| Test Estate 1 | Voucher      | Hospital 1 Contract | 10 KES         | 10 KES      |       | Voucher     |

	When I add the following Transaction Fees
	| EstateName    | OperatorName | ContractDescription | ProductName    | CalculationType | FeeDescription      | Value |
	| Test Estate 1 | Safaricom    | Safaricom Contract  | Variable Topup | Fixed           | Merchant Commission | 2.50  |

	Given I create the following merchants
	| MerchantName    | AddressLine1   | Town     | Region      | Country        | ContactName    | EmailAddress                 | EstateName    |
	| Test Merchant 1 | Address Line 1 | TestTown | Test Region | United Kingdom | Test Contact 1 | testcontact1@merchant1.co.uk | Test Estate 1 |
	| Test Merchant 2 | Address Line 1 | TestTown | Test Region | United Kingdom | Test Contact 1 | testcontact1@merchant2.co.uk | Test Estate 1 |

	Given I have assigned the following  operator to the merchants
	| OperatorName | MerchantName    | MerchantNumber | TerminalNumber | EstateName    |
	| Safaricom    | Test Merchant 1 | 00000001       | 10000001       | Test Estate 1 |
	| Voucher      | Test Merchant 1 | 00000001       | 10000001       | Test Estate 1 |
	| Safaricom    | Test Merchant 2 | 00000002       | 10000002       | Test Estate 1 |
	| Voucher      | Test Merchant 2 | 00000002       | 10000002       | Test Estate 1 |

	Given I have assigned the following devices to the merchants
	| DeviceIdentifier | MerchantName    | EstateName    |
	| 123456780        | Test Merchant 1 | Test Estate 1 |
	| 123456781        | Test Merchant 2 | Test Estate 1 |

	When I add the following contracts to the following merchants
	| EstateName    | MerchantName    | ContractDescription |
	| Test Estate 1 | Test Merchant 1 | Safaricom Contract  |
	| Test Estate 1 | Test Merchant 1 | Hospital 1 Contract |
	| Test Estate 1 | Test Merchant 2 | Safaricom Contract  |
	| Test Estate 1 | Test Merchant 2 | Hospital 1 Contract |
	
	Given I make the following manual merchant deposits 
	| Reference | Amount  | DateTime | MerchantName    | EstateName    |
	| Deposit1  | 300.00 | Today    | Test Merchant 1 | Test Estate 1 |
	| Deposit1  | 300.00 | Today    | Test Merchant 2 | Test Estate 1 |

@PRTest
Scenario: Get File Import Log Details
	Given I have a file named 'SafarcomTopup1.txt' with the following contents
	| Column1 | Column2     | Column3 |
	| H       | 20210508    |         |
	| D       | 07777777775 | 100     |
	| T       | 1           |         |
	And I upload this file for processing
	| EstateName    | MerchantName    | FileProfileId                        | UserId                               |
	| Test Estate 1 | Test Merchant 1 | B2A59ABF-293D-4A6B-B81B-7007503C3476 | ABA59ABF-293D-4A6B-B81B-7007503C3476 |

	Given I have a file named 'SafarcomTopup2.txt' with the following contents
	| Column1 | Column2     | Column3 |
	| H       | 20210508    |         |
	| D       | 07777777776 | 150     |
	| T       | 1           |         |
	And I upload this file for processing
	| EstateName    | MerchantName    | FileProfileId                        | UserId                               |
	| Test Estate 1 | Test Merchant 2 | B2A59ABF-293D-4A6B-B81B-7007503C3476 | ABA59ABF-293D-4A6B-B81B-7007503C3476 |

	Given I have a file named 'VoucherIssue1.txt' with the following contents
	| Column1 | Column2    | Column3                      | Column4 |
	| H       | 20210508   |                              |         |
	| D       | Hospital 1 | 07777777775                  | 10      |
	| D       | Hospital 1 | testrecipient1@recipient.com | 10      |
	| T       | 1          |                              |         |
	And I upload this file for processing
	| EstateName    | MerchantName    | FileProfileId                        | UserId                               |
	| Test Estate 1 | Test Merchant 1 | 8806EDBC-3ED6-406B-9E5F-A9078356BE99 | ABA59ABF-293D-4A6B-B81B-7007503C3476 |

	When I get the 'Test Estate 1' import logs between 'Yesterday' and 'Today' the following data is returned
	| ImportLogDate | FileCount |
	| Today         | 3         |

	When I get the 'Test Estate 1' import log for 'Today' the following file information is returned
	| MerchantName    | OriginalFileName   | 
	| Test Merchant 1 | SafarcomTopup1.txt | 
	| Test Merchant 2 | SafarcomTopup2.txt | 
	| Test Merchant 1 | VoucherIssue1.txt  | 

	When I get the file 'SafarcomTopup1.txt' for Estate 'Test Estate 1' the following file information is returned
	| ProcessingCompleted | NumberOfLines | TotaLines | SuccessfulLines | IgnoredLines | FailedLines | NotProcessedLines |
	| True                | 3             | 3         | 1               | 2            | 0           | 0                 |

	When I get the file 'SafarcomTopup1.txt' for Estate 'Test Estate 1' the following file lines are returned
	| LineNumber | Data               | Result     |
	| 1          | H,20210508,        | Ignored    |
	| 2          | D,07777777775,100 | Successful |
	| 3          | T,1,               | Ignored    |

	When I get the file 'SafarcomTopup2.txt' for Estate 'Test Estate 1' the following file information is returned
	| ProcessingCompleted | NumberOfLines | TotaLines | SuccessfulLines | IgnoredLines | FailedLines | NotProcessedLines |
	| True                | 3             | 3         | 1               | 2            | 0           | 0                 |

	When I get the file 'SafarcomTopup2.txt' for Estate 'Test Estate 1' the following file lines are returned
	| LineNumber | Data               | Result     |
	| 1          | H,20210508,        | Ignored    |
	| 2          | D,07777777776,150 | Successful |
	| 3          | T,1,               | Ignored    |

	When I get the file 'VoucherIssue1.txt' for Estate 'Test Estate 1' the following file information is returned
	| ProcessingCompleted | NumberOfLines | TotaLines | SuccessfulLines | IgnoredLines | FailedLines | NotProcessedLines |
	| True                | 4             | 4         | 2               | 2            | 0           | 0                 |

	When I get the file 'VoucherIssue1.txt' for Estate 'Test Estate 1' the following file lines are returned
	| LineNumber | Data                                         | Result     |
	| 1          | H,20210508,,                                 | Ignored    |
	| 2          | D,Hospital 1,07777777775,10                  | Successful |
	| 3          | D,Hospital 1,testrecipient1@recipient.com,10 | Successful |
	| 4          | T,1,,                                        | Ignored    |

	

	
	