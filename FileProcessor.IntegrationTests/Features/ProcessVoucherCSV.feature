@base @shared @processvouchercsv
Feature: Process Voucher CSV Files

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
	| EstateName    | OperatorName | ContractDescription | ProductName      | DisplayText | Value | ProductType |
	| Test Estate 1 | Safaricom    | Safaricom Contract  | Variable Topup   | Custom      |       | MobileTopup |
	| Test Estate 1 | Voucher      | Hospital 1 Contract | Variable Voucher | Custom      |       | Voucher     |

	When I add the following Transaction Fees
	| EstateName    | OperatorName | ContractDescription | ProductName      | CalculationType | FeeDescription      | Value |
	| Test Estate 1 | Safaricom    | Safaricom Contract  | Variable Topup   | Fixed           | Merchant Commission | 2.50  |
	| Test Estate 1 | Voucher      | Hospital 1 Contract | Variable Voucher | Fixed           | Merchant Commission | 2.50  |

	Given I create the following merchants
	| MerchantName    | AddressLine1   | Town     | Region      | Country        | ContactName    | EmailAddress                 | EstateName    |
	| Test Merchant 1 | Address Line 1 | TestTown | Test Region | United Kingdom | Test Contact 1 | testcontact1@merchant1.co.uk | Test Estate 1 |

	Given I have assigned the following  operator to the merchants
	| OperatorName | MerchantName    | MerchantNumber | TerminalNumber | EstateName    |
	| Safaricom    | Test Merchant 1 | 00000001       | 10000001       | Test Estate 1 |
	| Voucher      | Test Merchant 1 | 00000001       | 10000001       | Test Estate 1 |

	Given I have assigned the following devices to the merchants
	| DeviceIdentifier | MerchantName    | EstateName    |
	| 123456780        | Test Merchant 1 | Test Estate 1 |

	When I add the following contracts to the following merchants
	| EstateName    | MerchantName    | ContractDescription |
	| Test Estate 1 | Test Merchant 1 | Safaricom Contract  |
	| Test Estate 1 | Test Merchant 1 | Hospital 1 Contract |

	Given I make the following manual merchant deposits 
	| Reference | Amount  | DateTime | MerchantName    | EstateName    |
	| Deposit1  | 300.00 | Today    | Test Merchant 1 | Test Estate 1 |

@Nightly2	
Scenario: Process Voucher File with 1 detail row for recipient email
	Given I have a file named 'VoucherIssue.txt' with the following contents
	| Column1 | Column2    | Column3                      | Column4 |
	| H       | 20210508   |                              |         |
	| D       | Hospital 1 | testrecipient1@recipient.com | 10      |
	| T       | 1          |                              |         |
	And I upload this file for processing
	| EstateName    | MerchantName    | FileProfileId                        | UserId                               |
	| Test Estate 1 | Test Merchant 1 | 8806EDBC-3ED6-406B-9E5F-A9078356BE99 | ABA59ABF-293D-4A6B-B81B-7007503C3476 |

	#When As merchant "Test Merchant 1" on Estate "Test Estate 1" I get my transactions 1 transaction should be returned

@Nightly2
Scenario: Process Voucher File with 1 detail row for recipient mobile
	Given I have a file named 'VoucherIssue.txt' with the following contents
	| Column1 | Column2    | Column3                      | Column4 |
	| H       | 20210508   |                              |         |
	| D       | Hospital 1 | 07777777775                  | 10      |
	| T       | 1          |                              |         |
	And I upload this file for processing
	| EstateName    | MerchantName    | FileProfileId                        | UserId                               |
	| Test Estate 1 | Test Merchant 1 | 8806EDBC-3ED6-406B-9E5F-A9078356BE99 | ABA59ABF-293D-4A6B-B81B-7007503C3476 |

	#When As merchant "Test Merchant 1" on Estate "Test Estate 1" I get my transactions 1 transaction should be returned

@Nightly2
Scenario: Process Voucher File with 2 detail rows
	Given I have a file named 'VoucherIssue.txt' with the following contents
	| Column1 | Column2    | Column3                      | Column4 |
	| H       | 20210508   |                              |         |
	| D       | Hospital 1 | 07777777775                  | 10      |
	| D       | Hospital 1 | testrecipient1@recipient.com | 10      |
	| T       | 1          |                              |         |
	And I upload this file for processing
	| EstateName    | MerchantName    | FileProfileId                        | UserId                               |
	| Test Estate 1 | Test Merchant 1 | 8806EDBC-3ED6-406B-9E5F-A9078356BE99 | ABA59ABF-293D-4A6B-B81B-7007503C3476 |

	#When As merchant "Test Merchant 1" on Estate "Test Estate 1" I get my transactions 2 transaction should be returned

@Nightly2
Scenario: Process 2 Voucher Files
	Given I have a file named 'VoucherIssue1.txt' with the following contents
	| Column1 | Column2    | Column3                      | Column4 |
	| H       | 20210508   |                              |         |
	| D       | Hospital 1 | 07777777775                  | 10      |
	| D       | Hospital 1 | testrecipient1@recipient.com | 10      |
	| T       | 1          |                              |         |
	And I upload this file for processing
	| EstateName    | MerchantName    | FileProfileId                        | UserId                               |
	| Test Estate 1 | Test Merchant 1 | 8806EDBC-3ED6-406B-9E5F-A9078356BE99 | ABA59ABF-293D-4A6B-B81B-7007503C3476 |

	Given I have a file named 'VoucherIssue2.txt' with the following contents
	| Column1 | Column2    | Column3                      | Column4 |
	| H       | 20210508   |                              |         |
	| D       | Hospital 1 | 07777777775                  | 20      |
	| D       | Hospital 1 | testrecipient1@recipient.com | 20      |
	| T       | 1          |                              |         |
	And I upload this file for processing
	| EstateName    | MerchantName    | FileProfileId                        | UserId                               |
	| Test Estate 1 | Test Merchant 1 | 8806EDBC-3ED6-406B-9E5F-A9078356BE99 | ABA59ABF-293D-4A6B-B81B-7007503C3476 |

	#When As merchant "Test Merchant 1" on Estate "Test Estate 1" I get my transactions 4 transaction should be returned

@PRTest2
@Nightly2
Scenario: Process Duplicate Voucher Topup File with 1 detail row
	Given I have a file named 'VoucherIssue1.txt' with the following contents
	| Column1 | Column2    | Column3                      | Column4 |
	| H       | 20210508   |                              |         |
	| D       | Hospital 1 | 07777777775                  | 10      |
	| D       | Hospital 1 | testrecipient1@recipient.com | 20      |
	| T       | 1          |                              |         |
	And I upload this file for processing
	| EstateName    | MerchantName    | FileProfileId                        | UserId                               |
	| Test Estate 1 | Test Merchant 1 | 8806EDBC-3ED6-406B-9E5F-A9078356BE99 | ABA59ABF-293D-4A6B-B81B-7007503C3476 |
	
	#When As merchant "Test Merchant 1" on Estate "Test Estate 1" I get my transactions 2 transaction should be returned

	Given I have a file named 'VoucherIssue2.txt' with the following contents
	| Column1 | Column2    | Column3                      | Column4 |
	| H       | 20210508   |                              |         |
	| D       | Hospital 1 | 07777777775                  | 10      |
	| D       | Hospital 1 | testrecipient1@recipient.com | 20      |
	| T       | 1          |                              |         |

	And I upload this file for processing an error should be returned indicating the file is a duplicate
	| EstateName    | MerchantName    | FileProfileId                        | UserId                               |
	| Test Estate 1 | Test Merchant 1 | 8806EDBC-3ED6-406B-9E5F-A9078356BE99 | ABA59ABF-293D-4A6B-B81B-7007503C3476 |