@base @shared
Feature: Process Topup CSV Files

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
	
Scenario: Process Safaricom Topup File with 1 detail row
	Given I have a file named 'SafarcomTopup.txt' with the following contents
	| Column1 | Column2     | Column3 |
	| H       | 20210508    |         |
	| D       | 07777777775 | 100     |
	| T       | 1           |         |
	And I upload this file for processing
	| EstateName    | MerchantName    | FileProfileId                        | UserId                               |
	| Test Estate 1 | Test Merchant 1 | B2A59ABF-293D-4A6B-B81B-7007503C3476 | ABA59ABF-293D-4A6B-B81B-7007503C3476 |

	#When As merchant "Test Merchant 1" on Estate "Test Estate 1" I get my transactions 1 transaction should be returned

Scenario: Process Safaricom Topup File with 2 detail rows
	Given I have a file named 'SafarcomTopup.txt' with the following contents
	| Column1 | Column2     | Column3 |
	| H       | 20210508    |         |
	| D       | 07777777775 | 100     |
	| D       | 07777777776 | 200     |
	| T       | 2           |         |
	And I upload this file for processing
	| EstateName    | MerchantName    | FileProfileId                        | UserId                               |
	| Test Estate 1 | Test Merchant 1 | B2A59ABF-293D-4A6B-B81B-7007503C3476 | ABA59ABF-293D-4A6B-B81B-7007503C3476 |

	#When As merchant "Test Merchant 1" on Estate "Test Estate 1" I get my transactions 2 transaction should be returned

Scenario: Process 2 Safaricom Topup Files
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
	| D       | 07777777777 | 50      |
	| T       | 2           |         |
	And I upload this file for processing
	| EstateName    | MerchantName    | FileProfileId                        | UserId                               |
	| Test Estate 1 | Test Merchant 1 | B2A59ABF-293D-4A6B-B81B-7007503C3476 | ABA59ABF-293D-4A6B-B81B-7007503C3476 |

	#When As merchant "Test Merchant 1" on Estate "Test Estate 1" I get my transactions 3 transaction should be returned

@PRTest
Scenario: Process Duplicate Safaricom Topup File with 1 detail row
	Given I have a file named 'SafarcomTopup1.txt' with the following contents
	| Column1 | Column2     | Column3 |
	| H       | 20210508    |         |
	| D       | 07777777775 | 100     |
	| D       | 07777777776 | 200     |
	| T       | 1           |         |
	And I upload this file for processing
	| EstateName    | MerchantName    | FileProfileId                        | UserId                               |
	| Test Estate 1 | Test Merchant 1 | B2A59ABF-293D-4A6B-B81B-7007503C3476 | ABA59ABF-293D-4A6B-B81B-7007503C3476 |
	
	#When As merchant "Test Merchant 1" on Estate "Test Estate 1" I get my transactions 2 transaction should be returned

	Given I have a file named 'SafarcomTopup2.txt' with the following contents
	| Column1 | Column2     | Column3 |
	| H       | 20210508    |         |
	| D       | 07777777775 | 100     |
	| D       | 07777777776 | 200     |
	| T       | 1           |         |
	And I upload this file for processing an error should be returned indicating the file is a duplicate
	| EstateName    | MerchantName    | FileProfileId                        | UserId                               |
	| Test Estate 1 | Test Merchant 1 | B2A59ABF-293D-4A6B-B81B-7007503C3476 | ABA59ABF-293D-4A6B-B81B-7007503C3476 |
	
# Wrong Format??

Scenario: Process Safaricom Topup File with Upload Date Time
	Given I have a file named 'SafarcomTopup.txt' with the following contents
	| Column1 | Column2     | Column3 |
	| H       | 20210508    |         |
	| D       | 07777777775 | 100     |
	| T       | 1           |         |
	And I upload this file for processing
	| EstateName    | MerchantName    | FileProfileId                        | UserId                               | UploadDateTime |
	| Test Estate 1 | Test Merchant 1 | B2A59ABF-293D-4A6B-B81B-7007503C3476 | ABA59ABF-293D-4A6B-B81B-7007503C3476 | Today          |

	When I get the import log for estate 'Test Estate 1' the date on the import log is 'Today'

	Given I have a file named 'SafarcomTopup1.txt' with the following contents
	| Column1 | Column2     | Column3 |
	| H       | 20210508    |         |
	| D       | 07777777775 | 200     |
	| T       | 1           |         |
	And I upload this file for processing
	| EstateName    | MerchantName    | FileProfileId                        | UserId                               | UploadDateTime |
	| Test Estate 1 | Test Merchant 1 | B2A59ABF-293D-4A6B-B81B-7007503C3476 | ABA59ABF-293D-4A6B-B81B-7007503C3476 | 01/09/2021     |

	When I get the import log for estate 'Test Estate 1' the date on the import log is '01/09/2021'