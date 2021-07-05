@base @shared  @processtopupcsv @getfileimportdetails
Feature: GetFileImportDetails

Background: 
	Given I create the following api scopes
	| Name                 | DisplayName                       | Description                            |
	| estateManagement     | Estate Managememt REST Scope      | A scope for Estate Managememt REST     |
	| transactionProcessor | Transaction Processor REST  Scope | A scope for Transaction Processor REST |
	| voucherManagement | Voucher Management REST  Scope | A scope for Voucher Management REST |
	| fileProcessor | File Processor REST Scope | A scope for File Processor REST |

	Given the following api resources exist
	| ResourceName         | DisplayName                | Secret  | Scopes               | UserClaims                 |
	| estateManagement     | Estate Managememt REST     | Secret1 | estateManagement     | MerchantId, EstateId, role |
	| transactionProcessor | Transaction Processor REST | Secret1 | transactionProcessor |                            |
	| voucherManagement    | Voucher Management REST    | Secret1 | voucherManagement    |                            |
	| fileProcessor        | File Processor REST        | Secret1 | fileProcessor        |                            |

	Given the following clients exist
	| ClientId      | ClientName     | Secret  | AllowedScopes    | AllowedGrantTypes  |
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

	Given I create a contract with the following values
	| EstateName    | OperatorName | ContractDescription |
	| Test Estate 1 | Safaricom    | Safaricom Contract  |
	| Test Estate 1 | Voucher      | Hospital 1 Contract |

	When I create the following Products
	| EstateName    | OperatorName | ContractDescription | ProductName    | DisplayText | Value |
	| Test Estate 1 | Safaricom    | Safaricom Contract  | Variable Topup | Custom      |       |
	| Test Estate 1 | Voucher      | Hospital 1 Contract | 10 KES         | 10 KES      |       |

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

	Given I make the following manual merchant deposits 
	| Reference | Amount  | DateTime | MerchantName    | EstateName    |
	| Deposit1  | 300.00 | Today    | Test Merchant 1 | Test Estate 1 |
	| Deposit1  | 300.00 | Today    | Test Merchant 2 | Test Estate 1 |

@PRTest
Scenario: Get File Import Log Details
	Given I have a safaricom topup file with the following contents
	| Column1 | Column2     | Column3 |
	| H       | 20210508    |         |
	| D       | 07777777775 | 100     |
	| T       | 1           |         |
	And I upload this file for processing
	| EstateName    | MerchantName    | FileProfileId                        | UserId                               |
	| Test Estate 1 | Test Merchant 1 | B2A59ABF-293D-4A6B-B81B-7007503C3476 | ABA59ABF-293D-4A6B-B81B-7007503C3476 |

	Given I have a safaricom topup file with the following contents
	| Column1 | Column2     | Column3 |
	| H       | 20210508    |         |
	| D       | 07777777775 | 150     |
	| T       | 1           |         |
	And I upload this file for processing
	| EstateName    | MerchantName    | FileProfileId                        | UserId                               |
	| Test Estate 1 | Test Merchant 2 | B2A59ABF-293D-4A6B-B81B-7007503C3476 | ABA59ABF-293D-4A6B-B81B-7007503C3476 |

	When I get the 'Test Estate 1' import log for 'Today' the following data is returned
	| ImportLogDate | FileCount |
	| Today         | 2         |
	