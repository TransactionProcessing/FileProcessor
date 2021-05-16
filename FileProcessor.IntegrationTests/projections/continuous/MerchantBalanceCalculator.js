var fromCategory = fromCategory || require('../../node_modules/event-store-projection-testing').scope.fromCategory;
var partitionBy = partitionBy !== null ? partitionBy : require('../../node_modules/event-store-projection-testing').scope.partitionBy;
var emit = emit || require('../../node_modules/event-store-projection-testing').scope.emit;

fromCategory('MerchantArchive')
    .foreachStream()
    .when({
        $init: function () {
            return {
                initialised: true,
                availableBalance: 0,
                balance: 0,
                lastDepositDateTime: null,
                lastSaleDateTime: null,
                lastFeeProcessedDateTime: null,
                debug: [],
                totalDeposits: 0,
                totalAuthorisedSales: 0,
                totalDeclinedSales: 0,
                totalFees: 0,
                emittedEvents:1
            }
        },
        $any: function (s, e) {

            if (e === null || e.data === null || e.data.IsJson === false)
                return;

            eventbus.dispatch(s, e);
        }
    });

var eventbus = {
    dispatch: function (s, e) {

        if (e.eventType === 'MerchantCreatedEvent') {
            merchantCreatedEventHandler(s, e);
            return;
        }

        if (e.eventType === 'ManualDepositMadeEvent') {
            depositMadeEventHandler(s, e);
            return;
        }

        if (e.eventType === 'TransactionHasStartedEvent') {
            transactionHasStartedEventHandler(s, e);
            return;
        }

        if (e.eventType === 'TransactionHasBeenCompletedEvent') {
            transactionHasCompletedEventHandler(s, e);
            return;
        }

        if (e.eventType === 'MerchantFeeAddedToTransactionEvent') {
            merchantFeeAddedToTransactionEventHandler(s, e);
            return;
        }
    }
}

function getStreamName(s) {
    return "MerchantBalanceHistory-" + s.merchantId.replace(/-/gi, "");
}

function getEventTypeName() {
    return 'EstateReporting.BusinessLogic.Events.' + getEventType() + ', EstateReporting.BusinessLogic.Events';
}

function getEventType() { return "MerchantBalanceChangedEvent"; }

function addTwoNumbers(number1, number2) {
    return parseFloat((number1 + number2).toFixed(4));
}

function subtractTwoNumbers(number1, number2) {
    return parseFloat((number1 - number2).toFixed(4));
}

var incrementBalanceFromDeposit = function (s, amount, dateTime) {
    s.balance = addTwoNumbers(s.balance, amount);
    s.availableBalance = addTwoNumbers(s.availableBalance, amount);
    s.totalDeposits = addTwoNumbers(s.totalDeposits, amount);

    // protect against events coming in out of order
    if (s.lastDepositDateTime === null || dateTime > s.lastDepositDateTime) {
        s.lastDepositDateTime = dateTime;
    }
};

var incrementBalanceFromMerchantFee = function (s, amount, dateTime) {
    s.balance = addTwoNumbers(s.balance, amount);
    s.availableBalance = addTwoNumbers(s.availableBalance, amount);
    s.totalFees = addTwoNumbers(s.totalFees, amount);

    // protect against events coming in out of order
    if (s.lastFeeProcessedDateTime === null || dateTime > s.lastFeeProcessedDateTime) {
        s.lastFeeProcessedDateTime = dateTime;
    }
};

var decrementAvailableBalanceFromTransactionStarted = function (s, amount, dateTime) {
    s.availableBalance = subtractTwoNumbers(s.availableBalance, amount);

    // protect against events coming in out of order
    if (s.lastSaleDateTime === null || dateTime > s.lastSaleDateTime) {
        s.lastSaleDateTime = dateTime;
    }
};

var decrementBalanceFromAuthorisedTransaction = function (s, amount) {
    s.balance = subtractTwoNumbers(s.balance, amount);
    s.totalAuthorisedSales = addTwoNumbers(s.totalAuthorisedSales, amount);
};

var incrementAvailableBalanceFromDeclinedTransaction = function (s, amount) {
    s.availableBalance = addTwoNumbers(s.availableBalance, amount);
    s.totalDeclinedSales = addTwoNumbers(s.totalDeclinedSales, amount);
};

var merchantCreatedEventHandler = function (s, e) {

    // Setup the state here
    s.estateId = e.data.estateId;
    s.merchantId = e.data.merchantId;
    s.merchantName = e.data.merchantName;
};

var emitBalanceChangedEvent = function (aggregateId, eventId, s, changeAmount, dateTime, reference) {

    if (s.initialised === true) {
        
        // Emit an opening balance event
        var openingBalanceEvent = {
            $type: getEventTypeName(),
            "merchantId": s.merchantId,
            "estateId": s.estateId,
            "balance": 0,
            "changeAmount": 0,
            "eventId": s.merchantId,
            "eventCreatedDateTime": dateTime,
            "reference": "Opening Balance",
            "aggregateId": s.merchantId
        }
        emit(getStreamName(s), getEventType(), openingBalanceEvent);
        s.emittedEvents++;
        s.initialised = false;
    }

    var balanceChangedEvent = {
        $type: getEventTypeName(),
        "merchantId": s.merchantId,
        "estateId": s.estateId,
        "balance": s.balance,
        "changeAmount": changeAmount,
        "eventId": eventId,
        "eventCreatedDateTime": dateTime,
        "reference": reference,
        "aggregateId": aggregateId
    }

    // emit an balance changed event here
    emit(getStreamName(s), getEventType(), balanceChangedEvent);
    s.emittedEvents++;
    return s;
};

var depositMadeEventHandler = function (s, e) {

    // Check if we have got a merchant id already set
    if (s.merchantId === undefined) {
        // We have obviously not got a created event yet but we must process this event,
        // so fill in what we can here
        s.estateId = e.data.estateId;
        s.merchantId = e.data.merchantId;
    }

    incrementBalanceFromDeposit(s, e.data.amount, e.data.depositDateTime);

    var eventId = createEventId(JSON.stringify(e.data));
    // emit an balance changed event here
    s = emitBalanceChangedEvent(e.data.merchantId, eventId, s, e.data.amount, e.data.depositDateTime, "Merchant Deposit");
};

var transactionHasStartedEventHandler = function (s, e) {

    // Check if we have got a merchant id already set
    if (s.merchantId === undefined) {
        // We have obviously not got a created event yet but we must process this event,
        // so fill in what we can here
        e.estateId = e.data.estateId;
        s.merchantId = e.data.merchantId;
    }

    var amount = e.data.transactionAmount;
    if (amount === undefined) {
        amount = 0;
    }
    decrementAvailableBalanceFromTransactionStarted(s, amount, e.data.transactionDateTime);
};

var transactionHasCompletedEventHandler = function (s, e) {

    // Check if we have got a merchant id already set
    if (s.merchantId === undefined) {
        // We have obviously not got a created event yet but we must process this event,
        // so fill in what we can here
        e.estateId = e.data.estateId;
        s.merchantId = e.data.merchantId;
    }

    var amount = e.data.transactionAmount;
    if (amount === undefined) {
        amount = 0;
    }

    var transactionDateTime = new Date(Date.parse(e.data.completedDateTime));
    var completedTime = new Date(transactionDateTime.getFullYear(), transactionDateTime.getMonth(), transactionDateTime.getDate(), transactionDateTime.getHours(), transactionDateTime.getMinutes(), transactionDateTime.getSeconds() + 2);

    if (e.data.isAuthorised) {
        decrementBalanceFromAuthorisedTransaction(s, amount, completedTime);

        // emit an balance changed event here
        if (amount > 0) {
            var eventId = createEventId(JSON.stringify(e.data));
            s = emitBalanceChangedEvent(e.data.transactionId, eventId, s, amount * -1, completedTime, "Transaction Completed");
        }
    }
    else {
        incrementAvailableBalanceFromDeclinedTransaction(s, amount, completedTime);
    }
};

var merchantFeeAddedToTransactionEventHandler = function (s, e) {

    // Check if we have got a merchant id already set
    if (s.merchantId === undefined) {
        // We have obviously not got a created event yet but we must process this event,
        // so fill in what we can here
        e.estateId = e.data.estateId;
        s.merchantId = e.data.merchantId;
    }

    // increment the balance now
    incrementBalanceFromMerchantFee(s, e.data.calculatedValue, e.data.feeCalculatedDateTime);

    var eventId = createEventId(JSON.stringify(e.data));
    // emit an balance changed event here
    s = emitBalanceChangedEvent(e.data.transactionId, eventId, s, e.data.calculatedValue, e.data.feeCalculatedDateTime, "Transaction Fee Processed");
}

function createEventId(msg) {
    var hash = md5(msg, true);
    var uuid = hash.substring(0, 8) +
        '-' +
        hash.substring(8, 12) +
        '-' +
        hash.substring(12, 16) +
        '-' +
        hash.substring(16, 20) +
        '-' +
        hash.substring(20, 32);
    return uuid;
}
function md5(str) {

    var RotateLeft = function (lValue, iShiftBits) {
        return (lValue << iShiftBits) | (lValue >>> (32 - iShiftBits));
    };

    var AddUnsigned = function (lX, lY) {
        var lX4, lY4, lX8, lY8, lResult;
        lX8 = (lX & 0x80000000);
        lY8 = (lY & 0x80000000);
        lX4 = (lX & 0x40000000);
        lY4 = (lY & 0x40000000);
        lResult = (lX & 0x3FFFFFFF) + (lY & 0x3FFFFFFF);
        if (lX4 & lY4) {
            return (lResult ^ 0x80000000 ^ lX8 ^ lY8);
        }
        if (lX4 | lY4) {
            if (lResult & 0x40000000) {
                return (lResult ^ 0xC0000000 ^ lX8 ^ lY8);
            } else {
                return (lResult ^ 0x40000000 ^ lX8 ^ lY8);
            }
        } else {
            return (lResult ^ lX8 ^ lY8);
        }
    };

    var F = function (x, y, z) { return (x & y) | ((~x) & z); };
    var G = function (x, y, z) { return (x & z) | (y & (~z)); };
    var H = function (x, y, z) { return (x ^ y ^ z); };
    var I = function (x, y, z) { return (y ^ (x | (~z))); };

    var FF = function (a, b, c, d, x, s, ac) {
        a = AddUnsigned(a, AddUnsigned(AddUnsigned(F(b, c, d), x), ac));
        return AddUnsigned(RotateLeft(a, s), b);
    };

    var GG = function (a, b, c, d, x, s, ac) {
        a = AddUnsigned(a, AddUnsigned(AddUnsigned(G(b, c, d), x), ac));
        return AddUnsigned(RotateLeft(a, s), b);
    };

    var HH = function (a, b, c, d, x, s, ac) {
        a = AddUnsigned(a, AddUnsigned(AddUnsigned(H(b, c, d), x), ac));
        return AddUnsigned(RotateLeft(a, s), b);
    };

    var II = function (a, b, c, d, x, s, ac) {
        a = AddUnsigned(a, AddUnsigned(AddUnsigned(I(b, c, d), x), ac));
        return AddUnsigned(RotateLeft(a, s), b);
    };

    var ConvertToWordArray = function (str) {
        var lWordCount;
        var lMessageLength = str.length;
        var lNumberOfWords_temp1 = lMessageLength + 8;
        var lNumberOfWords_temp2 = (lNumberOfWords_temp1 - (lNumberOfWords_temp1 % 64)) / 64;
        var lNumberOfWords = (lNumberOfWords_temp2 + 1) * 16;
        var lWordArray = Array(lNumberOfWords - 1);
        var lBytePosition = 0;
        var lByteCount = 0;
        while (lByteCount < lMessageLength) {
            lWordCount = (lByteCount - (lByteCount % 4)) / 4;
            lBytePosition = (lByteCount % 4) * 8;
            lWordArray[lWordCount] = (lWordArray[lWordCount] | (str.charCodeAt(lByteCount) << lBytePosition));
            lByteCount++;
        }
        lWordCount = (lByteCount - (lByteCount % 4)) / 4;
        lBytePosition = (lByteCount % 4) * 8;
        lWordArray[lWordCount] = lWordArray[lWordCount] | (0x80 << lBytePosition);
        lWordArray[lNumberOfWords - 2] = lMessageLength << 3;
        lWordArray[lNumberOfWords - 1] = lMessageLength >>> 29;
        return lWordArray;
    };

    var WordToHex = function (lValue) {
        var WordToHexValue = "", WordToHexValue_temp = "", lByte, lCount;
        for (lCount = 0; lCount <= 3; lCount++) {
            lByte = (lValue >>> (lCount * 8)) & 255;
            WordToHexValue_temp = "0" + lByte.toString(16);
            WordToHexValue = WordToHexValue + WordToHexValue_temp.substr(WordToHexValue_temp.length - 2, 2);
        }
        return WordToHexValue;
    };

    var x = Array();
    var k, AA, BB, CC, DD, a, b, c, d;
    var S11 = 7, S12 = 12, S13 = 17, S14 = 22;
    var S21 = 5, S22 = 9, S23 = 14, S24 = 20;
    var S31 = 4, S32 = 11, S33 = 16, S34 = 23;
    var S41 = 6, S42 = 10, S43 = 15, S44 = 21;

    x = ConvertToWordArray(str);
    a = 0x67452301; b = 0xEFCDAB89; c = 0x98BADCFE; d = 0x10325476;

    for (k = 0; k < x.length; k += 16) {
        AA = a; BB = b; CC = c; DD = d;
        a = FF(a, b, c, d, x[k + 0], S11, 0xD76AA478);
        d = FF(d, a, b, c, x[k + 1], S12, 0xE8C7B756);
        c = FF(c, d, a, b, x[k + 2], S13, 0x242070DB);
        b = FF(b, c, d, a, x[k + 3], S14, 0xC1BDCEEE);
        a = FF(a, b, c, d, x[k + 4], S11, 0xF57C0FAF);
        d = FF(d, a, b, c, x[k + 5], S12, 0x4787C62A);
        c = FF(c, d, a, b, x[k + 6], S13, 0xA8304613);
        b = FF(b, c, d, a, x[k + 7], S14, 0xFD469501);
        a = FF(a, b, c, d, x[k + 8], S11, 0x698098D8);
        d = FF(d, a, b, c, x[k + 9], S12, 0x8B44F7AF);
        c = FF(c, d, a, b, x[k + 10], S13, 0xFFFF5BB1);
        b = FF(b, c, d, a, x[k + 11], S14, 0x895CD7BE);
        a = FF(a, b, c, d, x[k + 12], S11, 0x6B901122);
        d = FF(d, a, b, c, x[k + 13], S12, 0xFD987193);
        c = FF(c, d, a, b, x[k + 14], S13, 0xA679438E);
        b = FF(b, c, d, a, x[k + 15], S14, 0x49B40821);
        a = GG(a, b, c, d, x[k + 1], S21, 0xF61E2562);
        d = GG(d, a, b, c, x[k + 6], S22, 0xC040B340);
        c = GG(c, d, a, b, x[k + 11], S23, 0x265E5A51);
        b = GG(b, c, d, a, x[k + 0], S24, 0xE9B6C7AA);
        a = GG(a, b, c, d, x[k + 5], S21, 0xD62F105D);
        d = GG(d, a, b, c, x[k + 10], S22, 0x2441453);
        c = GG(c, d, a, b, x[k + 15], S23, 0xD8A1E681);
        b = GG(b, c, d, a, x[k + 4], S24, 0xE7D3FBC8);
        a = GG(a, b, c, d, x[k + 9], S21, 0x21E1CDE6);
        d = GG(d, a, b, c, x[k + 14], S22, 0xC33707D6);
        c = GG(c, d, a, b, x[k + 3], S23, 0xF4D50D87);
        b = GG(b, c, d, a, x[k + 8], S24, 0x455A14ED);
        a = GG(a, b, c, d, x[k + 13], S21, 0xA9E3E905);
        d = GG(d, a, b, c, x[k + 2], S22, 0xFCEFA3F8);
        c = GG(c, d, a, b, x[k + 7], S23, 0x676F02D9);
        b = GG(b, c, d, a, x[k + 12], S24, 0x8D2A4C8A);
        a = HH(a, b, c, d, x[k + 5], S31, 0xFFFA3942);
        d = HH(d, a, b, c, x[k + 8], S32, 0x8771F681);
        c = HH(c, d, a, b, x[k + 11], S33, 0x6D9D6122);
        b = HH(b, c, d, a, x[k + 14], S34, 0xFDE5380C);
        a = HH(a, b, c, d, x[k + 1], S31, 0xA4BEEA44);
        d = HH(d, a, b, c, x[k + 4], S32, 0x4BDECFA9);
        c = HH(c, d, a, b, x[k + 7], S33, 0xF6BB4B60);
        b = HH(b, c, d, a, x[k + 10], S34, 0xBEBFBC70);
        a = HH(a, b, c, d, x[k + 13], S31, 0x289B7EC6);
        d = HH(d, a, b, c, x[k + 0], S32, 0xEAA127FA);
        c = HH(c, d, a, b, x[k + 3], S33, 0xD4EF3085);
        b = HH(b, c, d, a, x[k + 6], S34, 0x4881D05);
        a = HH(a, b, c, d, x[k + 9], S31, 0xD9D4D039);
        d = HH(d, a, b, c, x[k + 12], S32, 0xE6DB99E5);
        c = HH(c, d, a, b, x[k + 15], S33, 0x1FA27CF8);
        b = HH(b, c, d, a, x[k + 2], S34, 0xC4AC5665);
        a = II(a, b, c, d, x[k + 0], S41, 0xF4292244);
        d = II(d, a, b, c, x[k + 7], S42, 0x432AFF97);
        c = II(c, d, a, b, x[k + 14], S43, 0xAB9423A7);
        b = II(b, c, d, a, x[k + 5], S44, 0xFC93A039);
        a = II(a, b, c, d, x[k + 12], S41, 0x655B59C3);
        d = II(d, a, b, c, x[k + 3], S42, 0x8F0CCC92);
        c = II(c, d, a, b, x[k + 10], S43, 0xFFEFF47D);
        b = II(b, c, d, a, x[k + 1], S44, 0x85845DD1);
        a = II(a, b, c, d, x[k + 8], S41, 0x6FA87E4F);
        d = II(d, a, b, c, x[k + 15], S42, 0xFE2CE6E0);
        c = II(c, d, a, b, x[k + 6], S43, 0xA3014314);
        b = II(b, c, d, a, x[k + 13], S44, 0x4E0811A1);
        a = II(a, b, c, d, x[k + 4], S41, 0xF7537E82);
        d = II(d, a, b, c, x[k + 11], S42, 0xBD3AF235);
        c = II(c, d, a, b, x[k + 2], S43, 0x2AD7D2BB);
        b = II(b, c, d, a, x[k + 9], S44, 0xEB86D391);
        a = AddUnsigned(a, AA);
        b = AddUnsigned(b, BB);
        c = AddUnsigned(c, CC);
        d = AddUnsigned(d, DD);
    }

    var temp = WordToHex(a) + WordToHex(b) + WordToHex(c) + WordToHex(d);

    return temp.toLowerCase();
}