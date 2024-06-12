var dialogObj;
let total = 0
let count = 0
let billsSelected = new Array()
let bankAccount = ""
let bankAccountName = ""
let bankAccountCode = ""
var table;
var totalRowCount = 0;
var objSelectedBills;
var elementIDName = new Array();
let markAsPaidResult;
let disabledGridEditing = false



function modalCreated() {
    dialogObj = this;
    dialogObj.hide();
}

function dlgContainerBeforeOpen() {
    $("#dlgContainer").css("visibility", "visible");
}

function promptDlgContainerBeforeOpen() {
    $("#promptDlgContainer").css("visibility", "visible");
}

function promptContainerForMarkAsPaidBeforeOpen() {
    $("#promptContainerForMarkAsPaid").css("visibility", "visible");
}

function promptContainerBeforeOpen() {
    $("#promptContainer").css("visibility", "visible");
}

function promptContainerExpireSubscriptionsBeforeOpen() {
    $("#promptContainerExpireSubscriptions").css("visibility", "visible");
}

function promptContainerForABABeforeOpen() {
    $("#promptContainerForABA").css("visibility", "visible");
}

function promptDlgProgressContainerBeforeOpen() {
    $("#promptDlgProgressContainer").css("visibility", "visible");
}

function promptDlgContainerTransactionDateBeforeOpen() {
    $("#promptDlgContainerTransactionDate").css("visibility", "visible");
}

function errorDlgContainerBeforeOpen() {
    $("#errorDlgContainer").css("visibility", "visible");
}

function promptDlgContainerTransactionDate2() {
    $("#promptDlgContainerTransactionDate2").css("visibility", "visible");
}

function onDtChange(args) {

    if (args.value === null) {
        document.getElementById("messageTransactionDate").style.display = "block";
    } else {
        document.getElementById("messageTransactionDate").style.display = "none";
    }
}

function actionCompleteInvoiceGrid(args) {
    var grid = document.getElementById("GridSorting").ej2_instances[0];
    grid.hideSpinner()


    for (let i = 0; i < elementIDName.length; i++) {
        checkboxFromGrid = "#" + elementIDName[i];
        baseElement = document.querySelector(checkboxFromGrid);
        elementid = baseElement.id.toString();
        spanelement = baseElement.parentNode.querySelector('span')

        if (spanelement.classList.contains('e-uncheck') == true) {
            spanelement.classList.add("e-check");
            spanelement.classList.remove("e-uncheck");
        }

    }

    elementIDName.length = 0;
    elementIDName = new Array(); // []

    if (args.action === 'edit') {

        billsSelected = this.getSelectedRecords()
        total = 0;

        billsSelected.forEach((item) => {
            total += item.amountDueEditable
        })
        document.getElementById("total").innerHTML = "$" + total.toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,')

    }



    //function actionBeginInvoiceGrid(args) {
    //    var grid = document.getElementById("GridSorting").ej2_instances[0];

    //    if (args.action === 'edit') {

    //        billsSelected = this.getSelectedRecords()
    //        total = 0;

    //        billsSelected.forEach((item) => {
    //            total += item.amountDueEditable
    //        })
    //        document.getElementById("total").innerHTML = "$" + total.toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,')

    //    }
    //if (args.requestType === 'save') {
    //    if (grid.pageSettings.currentPage !== 1 && grid.editSettings.newRowPosition === 'Top') {
    //        args.index = (grid.pageSettings.currentPage * grid.pageSettings.pageSize) - grid.pageSettings.pageSize;
    //    } else if (grid.editSettings.newRowPosition === 'Bottom') {
    //        args.index = (grid.pageSettings.currentPage * grid.pageSettings.pageSize) - 1;
    //    }
    //}

    //if (args.requestType === 'delete') {
    //    ColumnNameSelected.splice(ColumnNameSelected.findIndex(sel => sel === args.data[0].ColumnName), 1)
    //    OrderNoSelected.splice(OrderNoSelected.findIndex(sel => sel === args.data[0].ColumnName), 1)
    //}
}



function padTo2Digits(num) {
    return num.toString().padStart(2, '0');
}

function formatDate(date) {
    return [
        padTo2Digits(date.getMonth() + 1),
        padTo2Digits(date.getDate()),
        date.getFullYear(),
    ].join('/');
}

function promptEmailAddressValidation(selectedInvoices) {

    var contactWithNoEmail = "";
    var filteredInvoices = [...new Set(selectedInvoices.filter(i => i.hasEmailAddress == false).map(item => item.contactName))];

    filteredInvoices.forEach(i => contactWithNoEmail += i + " has no email address\n");

    NumOfInvoice = selectedInvoices.length
    NumOfIssues = filteredInvoices.length

    var dialog = document.getElementById("promptDialogMarkAsPaid").ej2_instances[0]

    var titleFormat = "Mark as Paid";
    document.getElementById("fileFormat2").innerHTML = titleFormat;
    document.getElementById("promptMarkAsIssue").innerHTML = "You are about to generate " + titleFormat + " file containing " + NumOfInvoice + " invoices. The file however contains " + filteredInvoices.length + " errors.";

    var dlgHeader = document.getElementById("promptDialogMarkAsPaid_dialog-header")
    var dlgContent = document.getElementById("promptDialogMarkAsPaid_dialog-content")

    dlgHeader.classList.add("error-dlg-header-template")
    dlgContent.classList.add("dlg-content-template")

    //if (selectedInvoices.length === filteredInvoices.length) {
    //    document.getElementById("btnMarkAsPaidOkay").removeAttribute("editable");
    //    document.getElementById("btnMarkAsPaidOkay").setAttribute("disabled", true);
    //} else {
    //    document.getElementById("btnMarkAsPaidOkay").removeAttribute("disabled");
    //    document.getElementById("btnMarkAsPaidOkay").setAttribute("editable", true);
    //}

    dialog.show()

    document.getElementById("btnShowMarkAsPaidIssues").onclick = function (e) {
        var blob = new Blob([contactWithNoEmail], { type: "text/plain;charset=utf-8" });
        const downloadUrl = URL.createObjectURL(blob)
        const a = document.createElement("a")
        a.href = downloadUrl
        a.download = "errors_mark_as_paid.txt"
        document.body.appendChild(a)
        a.click()
        grid.hideSpinner()
    }

}

function CheckIfConnectedToXero() {
    var selectedTenant = $('select#orgSelector').find(':selected').data('id')
    var IsConnected = false
    $.ajax({
        url: '/Xero/CheckIfConnectedToXero/' + selectedTenant,
        type: 'GET',
        async: false,
        contentType: 'application/json',
        accept: 'application/json',
        success: (result) => {
            if (result.isError) {
                operations.errorHandling(result)
                return
            } else {
                IsConnected = result.isConnected
                return result.isConnected
            }
        },
        error: (error) => {
            console.log(error)
        }
    })
    return IsConnected
}

function tenantNotConnected() {

    //var contactWithNoEmail = "<br/> ";
    //var filteredInvoices = [...new Set(selectedInvoices.filter(i => i.hasEmailAddress == false).map(item => item.contactName))];
    //filteredInvoices.forEach(i => contactWithNoEmail += i + ", ");

    document.getElementById("progressStatus4").style.display = "none";
    document.getElementById("Prog4").style.display = "none";


    var dialog = document.getElementById("promptInfoDialog").ej2_instances[0]
    document.getElementById("promptContent").innerHTML = "Tenant is disonnected from xero."

    var dlgHeader = document.getElementById("promptInfoDialog_dialog-header")
    dlgHeader.classList.add("error-dlg-header-template")

    var dlgContent = document.getElementById("promptInfoDialog_dialog-content")
    dlgContent.classList.add("dlg-content-template")

    dlgContent.classList.add("dlg-content-template")

    document.getElementById("btnPromptCancel").style.display = "none";

    document.getElementById("btnPromptOkay").onclick = () => {
        SwitchLocation()
    }

    dialog.show()

}


function promptTransactionDate() {

    //var contactWithNoEmail = "<br/> ";
    //var filteredInvoices = [...new Set(selectedInvoices.filter(i => i.hasEmailAddress == false).map(item => item.contactName))];
    //filteredInvoices.forEach(i => contactWithNoEmail += i + ", ");

    document.getElementById("messageTransactionDate").style.display = "none";
    var d = new Date()
    var dateTransaction = document.getElementById("dateTransaction").ej2_instances[0]
    dateTransaction.value = d

    var dialog = document.getElementById("promptInfoDialogTransactionDate").ej2_instances[0]
    document.getElementById("promptContent").innerHTML = ""

    var dlgHeader = document.getElementById("promptInfoDialogTransactionDate_dialog-header")
    dlgHeader.classList.add("prompt-dlg-header-template")

    var dlgContent = document.getElementById("promptInfoDialogTransactionDate_dialog-content")
    dlgContent.classList.add("dlg-content-template")
    dialog.show()

}


function promptTransactionDate2() {

    //var contactWithNoEmail = "<br/> ";
    //var filteredInvoices = [...new Set(selectedInvoices.filter(i => i.hasEmailAddress == false).map(item => item.contactName))];
    //filteredInvoices.forEach(i => contactWithNoEmail += i + ", ");

    document.getElementById("messageTransactionDate").style.display = "none";
    var d = new Date()
    var dateTransaction = document.getElementById("dateTransaction2").ej2_instances[0]
    dateTransaction.value = d

    var dialog = document.getElementById("promptInfoDialogTransactionDate2").ej2_instances[0]
    document.getElementById("promptContent").innerHTML = ""

    var dlgHeader = document.getElementById("promptInfoDialogTransactionDate2_dialog-header")
    dlgHeader.classList.add("prompt-dlg-header-template")

    var dlgContent = document.getElementById("promptInfoDialogTransactionDate2_dialog-content")
    dlgContent.classList.add("dlg-content-template")
    dialog.show()

}


function promptBankDetails(selectedInvoices) {

    var contactWithNoEmail = "";
    selectedInvoices.filter(i => i.hasEmailAddress == false).forEach(i => contactWithNoEmail += ", " + i.contactName);

    var dialog = document.getElementById("promptInfoDialog").ej2_instances[0]
    document.getElementById("promptContent").innerHTML = "Contacts with no email address:" + contactWithNoEmail.replaceAll(",", "<br/>") + "<br />  <br />  Do you want to continue transaction?"

    var dlgHeader = document.getElementById("promptInfoDialog_dialog-header")
    dlgHeader.classList.add("prompt-dlg-header-template")

    var dlgContent = document.getElementById("promptInfoDialog_dialog-content")
    dlgContent.classList.add("dlg-content-template")
    dialog.show()

}

function keyDownHandler(e) {
    //if (e.keyCode === 13) {
    //    var gridIns = grid.ej2_instances[0];
    //    gridIns.addRecord();
    //}
    console.log(e.keyCode)
}

function isCharNumber(c) {
    return c >= '0' && c <= '9';
}


document.addEventListener("DOMContentLoaded", function () {
    let grid = document.getElementById("Grid").ej2_instances[0]
    grid.addEventListener("keydown", keyDownHandler);
    grid.width = window.width
    grid.height = window.height
    document.getElementById("selectedBills").innerHTML = count
    document.getElementById("total").innerHTML = total
    console.log('DOMContentLoaded OKOK')
    operations.onFilterBillsSelected()
    operations.onGetBills()
})


window.addEventListener('load', () => {
    console.log('window load OK')

    let orgs = "";
    if (expireOrganization.length > 0) {

        setTimeout(function () {
            var dialogExpireOrgDialog = document.getElementById("promptExpireDialog").ej2_instances[0];

            var content = "Hello, there";
            var dataArray = content.split(",");
            console.log(dataArray);
            console.log(dataArray.join("\n"));


            var content = expireOrganization;
            var dataArray = content.split(",");
            console.log(dataArray);
            console.log(dataArray.join("\n"));

            dataArray.forEach(myFunction)

            function myFunction(item, index) {
                orgs += "<b>.</b> " + item + "<br>";
            }

            document.getElementById("prompt2").innerHTML = "The organisation/s subscriptions expires <br><br>" + orgs;
            dialogExpireOrgDialog.show();

            document.getElementById("btnOK").onclick = function (e) {
                dialogExpireOrgDialog.hide()
            }
        }, 5000);

    }

})

let operations = {
    onGetBills: function () {
        $('#listReportBug').attr('disabled', 'disabled');

        var grid = document.getElementById("Grid").ej2_instances[0]
        grid.showSpinner()

        let payload = {
            page: 1,
        }

        if (CheckIfConnectedToXero()) {
            var ajax = new ej.base.Ajax({
                url: '/Xero/FilterBills',
                type: 'POST',
                data: JSON.stringify(payload),
                contentType: 'application/json',
                accept: 'application/json'
            })
            ajax.send()
            ajax.onSuccess = function (data) {

                var obj = JSON.parse(data)

                if (obj.isError) {
                    operations.errorHandling(obj)
                    return
                }
      
                grid.dataSource = obj.invoices
                grid.hideSpinner()
                document.getElementById("total").innerHTML = 0
                document.getElementById("selectedBills").innerHTML = 0
                grid.clearCellSelection()
                grid.clearRowSelection()
                grid.clearSelection()
                grid.selectionModule.deSelectedData = []
                grid.selectionModule.persistSelectedData = []
                $('#listReportBug').removeAttr('disabled');

            }
        }
        else
            tenantNotConnected()


    },
    onFilterBillsSelected: function () {

        if (CheckIfConnectedToXero()) {
            var tranasctionType = "ExportDataFormat"
            $.ajax({
                url: '/Xero/FilterBillsSelected/' + tranasctionType,
                type: 'GET',
                async: false,
                contentType: 'application/json',
                accept: 'application/json',
                success: (result) => {
                    objSelectedBills = JSON.parse(result)
                },
                error: (error) => {
                    console.log(error)
                }
            })
        }
        else
            tenantNotConnected()


    },
    onGridDataBound: function (args) {

        if (objSelectedBills == null) {
            //no action performed
        } else {

            var invoiceSearch = objSelectedBills.Invoices.find(e => e.InvoiceID === args.data.invoiceID)

            if (invoiceSearch != null) {
                var cellInnerHtml = args.row.cells[0].innerHTML;

                //check for 3 digits or 4 digits
                if (isCharNumber(cellInnerHtml.substring(128, 129)) == false) {
                    elementIDName.push(cellInnerHtml.substring(109, 128));
                } else {

                    if (isCharNumber(cellInnerHtml.substring(129, 130)) == false) {
                        elementIDName.push(cellInnerHtml.substring(109, 129));
                    } else {
                        elementIDName.push(cellInnerHtml.substring(109, 130));
                    }
                }
            }
        }


    },
    onFilterBillStatus: function (args) {
        let dateRange = document.getElementById("invDateRangeFilter").ej2_instances[0]

        var grid = document.getElementById("Grid").ej2_instances[0]
        grid.showSpinner()

        let payload = {
            status: args.value,
            startDate: new Date(dateRange.startValue),
            endDate: new Date(dateRange.endValue)
        }

        if (CheckIfConnectedToXero()) {
            var ajax = new ej.base.Ajax({
                url: '/Xero/FilterBills',
                type: 'POST',
                data: JSON.stringify(payload),
                contentType: 'application/json',
                accept: 'application/json'
            })
            ajax.send()
            ajax.onSuccess = function (data) {
                var obj = JSON.parse(data)
                if (obj.isError) {
                    operations.errorHandling(obj)
                    return
                }
                grid.dataSource = obj.invoices
                grid.hideSpinner()
            }
        }
        else
            tenantNotConnected()

    },
    onToolbarClick: function (args) {
        if (args.item.id === 'Export') {

            var isinsertRecordDB;

            document.getElementById("progressStatus").innerHTML = "<span style='color:green;'><b>Status:</b>  Selecting export date...</span>";
            var count = 0;
            setInterval(function () {

                if (count <= 25) {
                    document.getElementById('Prog').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';
                } else {
                    return;
                }
                count += 1;
            }, 100);

            var grid = document.getElementById("Grid").ej2_instances[0]
            var dateTransaction = document.getElementById("dateTransaction").ej2_instances[0]
            grid.showSpinner()

            let selectedInvoices

            if (this.getSelectedRecords().length === 0) {
                selectedInvoices = objSelectedBills.Invoices
                isinsertRecordDB = "0";
            } else {
                selectedInvoices = this.getSelectedRecords();
                isinsertRecordDB = "1";
            }

            if (selectedInvoices.length === 0) {
                var message = "";

                if (selectedInvoices.length === 0) {
                    message += "* Please select invoices to export. <br/><br/>"
                }

                var dialog = document.getElementById("dialog").ej2_instances[0]
                document.getElementById("message").innerHTML = message

                var dlgHeader = document.getElementById("dialog_dialog-header")
                var dlgContent = document.getElementById("dialog_dialog-content")

                dlgHeader.classList.add("error-dlg-header-template")
                dlgContent.classList.add("dlg-content-template")

                dialog.show()
                grid.hideSpinner()
            } else {

                promptTransactionDate();
                document.getElementById("btnPromptOkayTransactionDate").onclick = () => {


                    if (dateTransaction.value === null) {

                        return
                    }

                    var dialog = document.getElementById("promptInfoDialogTransactionDate").ej2_instances[0]
                    dialog.hide()

                    var promptDialog = document.getElementById("promptDialog").ej2_instances[0]
                    document.getElementById("prompt").innerHTML = `Please confirm you’d like to proceed updating ${selectedInvoices.length} invoices in Xero to populate bank account in ABA file`

                    var dlgHeader = document.getElementById("promptDialog_dialog-header")
                    dlgHeader.classList.add("prompt-dlg-header-template")

                    var dlgContent = document.getElementById("promptDialog_dialog-content")
                    dlgContent.classList.add("dlg-content-template")

                    var bankDropDown = document.getElementById("bankAccountTypesDropDown").ej2_instances[0]

                    document.getElementById("errorContentBank").innerHTML = ''
                    promptDialog.show()
                    document.getElementById("progressStatus2").innerHTML = "<span style='color:green;'><b>Status:</b>  Selecting Bank Account...</span>";

                    var count = 25;
                    setInterval(function () {

                        if (count <= 50) {
                            document.getElementById('Prog2').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';
                        } else {
                            return;
                        }
                        count += 1;
                    }, 100);


                    $.ajax({
                        url: '/Xero/GetBankAccountTypes',
                        type: 'GET',
                        headers: {
                            'Accept': 'application/json; charset=utf-8'
                        },
                        success: (result) => {
                            if (result.isError) {
                                operations.errorHandling(result)
                                return
                            }
                            else {

                                if (result.bankAccounts.length > 0)
                                    bankDropDown.dataSource = result.bankAccounts
                                else
                                    document.getElementById("errorContentBank").innerHTML = 'No bank account'
                            }
                        },
                        error: (error) => {
                            document.getElementById("errorContentBank").innerHTML = [error.statusText, error.responseText].join(' ')
                        }
                    })

                    document.getElementById("btnOkay").onclick = function (e) {

                        if (bankAccount === null || bankAccountName == null || bankAccount.length === 0 || bankAccountName.length === 0)
                            return

                        var dialogProgress1 = document.getElementById("prompProgressDialog").ej2_instances[0]

                        var dlgHeader = document.getElementById("prompProgressDialog_dialog-header")
                        var dlgContent = document.getElementById("prompProgressDialog_dialog-content")

                        dlgHeader.classList.add("prompt-dlg-header-template")
                        dlgContent.classList.add("dlg-content-template")

                        dialogProgress1.show()

                        document.getElementById("progressStatusOnly").innerHTML = "<span style='color:green;'><b>Status:</b>  Bank Account Selected...</span>";

                        var count = 50;
                        setInterval(function () {

                            if (count <= 65) {
                                document.getElementById('ProgOnly').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';
                            } else {
                                return;
                            }
                            count += 1;
                        }, 100);

                        let errorCount = 0;
                        promptDialog.hide()

                        let exportToABA = {
                            bankAccountNumber: bankAccount,
                            bankAccountName: bankAccountName,
                            bankAccountCode: bankAccountCode,
                            errorCount: errorCount,
                            invoices: selectedInvoices,
                            isinsertRecordDB: isinsertRecordDB,
                            TransactionDate: formatDate(dateTransaction.value)
                        }
                        if (CheckIfConnectedToXero()) {
                            fetch('/Xero/ExportToAbaFormatChecking', {
                                body: JSON.stringify(exportToABA),
                                method: 'POST',
                                headers: {
                                    'Content-Type': 'application/json; charset=utf-8'
                                },
                            }).then(response => response.json())
                                .then(response => {
                                    if (response.result == false) {
                                        var dialog = document.getElementById("promptDialogABA").ej2_instances[0]


                                        var fileFormat = "ABA";
                                        var titleFormat = "";

                                        switch (countryCode) {
                                            case 'NZ':
                                                fileFormat = "a CSV";
                                                titleFormat = "CSV"
                                                break;
                                            case 'AU':
                                                fileFormat = "an ABA";
                                                titleFormat = "ABA"
                                                break;
                                            case 'GB':
                                                fileFormat = "a BACS";
                                                titleFormat = "BACS"
                                                break;
                                        }

                                        var errorlist = "";
                                        for (let i = 0; i < response.error.length; i++) {
                                            errorlist += response.error[i].error + "\n";
                                            errorCount++;
                                        }

                                        document.getElementById("fileFormat").innerHTML = titleFormat;
                                        document.getElementById("prompt").innerHTML = "You are about to generate " + fileFormat + " file containing " + response.countInvoice + " invoices. The file however contains " + response.countIssue + " errors.";
                                        var dlgHeader = document.getElementById("promptDialogABA_dialog-header")
                                        var dlgContent = document.getElementById("promptDialogABA_dialog-content")

                                        dlgHeader.classList.add("error-dlg-header-template")
                                        dlgContent.classList.add("dlg-content-template")

                                        if (selectedInvoices.length === response.error.length) {
                                            document.getElementById("btnABAOkay").removeAttribute("editable");
                                            document.getElementById("btnABAOkay").setAttribute("disabled", true);
                                        } else {
                                            document.getElementById("btnABAOkay").removeAttribute("disabled");
                                            document.getElementById("btnABAOkay").setAttribute("editable", true);
                                        }

                                        dialogProgress1.hide()
                                        dialog.show()

                                        var count = 65;
                                        setInterval(function () {

                                            if (count <= 75) {
                                                document.getElementById('Prog3').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';
                                            } else {
                                                return;
                                            }
                                            count += 1;
                                        }, 100);

                                        document.getElementById("progressStatus3").innerHTML = "<span style='color:green;'><b>Status:</b>  Exporting data to ABA...</span>";

                                        grid.hideSpinner()
                                        promptDialog.hide()


                                        document.getElementById("btnABAOkay").onclick = function (e) {

                                            var count = 75;
                                            setInterval(function () {

                                                if (count <= 100) {
                                                    document.getElementById('Prog3').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';
                                                } else {
                                                    return;
                                                }
                                                count += 1;
                                            }, 200);

                                            document.getElementById("progressStatus3").innerHTML = "<span style='color:green;'><b>Status:</b>  Exporting data to ABA...</span>";


                                            let exportToABA2 = {
                                                bankAccountNumber: bankAccount,
                                                bankAccountName: bankAccountName,
                                                bankAccountCode: bankAccountCode,
                                                errorCount: errorCount,
                                                invoices: selectedInvoices,
                                                isinsertRecordDB: isinsertRecordDB,
                                                TransactionDate: formatDate(dateTransaction.value)
                                            }

                                            fetch('/Xero/ExportToAbaFormat', {
                                                body: JSON.stringify(exportToABA2),
                                                method: 'POST',
                                                headers: {
                                                    'Content-Type': 'application/json; charset=utf-8'
                                                },
                                            }).then(response => response.blob())
                                                .then(response => {
                                                    const blob = new Blob([response], { type: 'text/plain' })
                                                    const downloadUrl = URL.createObjectURL(blob)
                                                    const a = document.createElement("a")
                                                    a.href = downloadUrl
                                                    a.download = "batch_payments_export.aba"
                                                    document.body.appendChild(a)
                                                    dialog.hide()
                                                    grid.hideSpinner()
                                                    a.click()
                                                    disabledGridEditing = true
                                                    document.getElementById("Grid").ej2_instances[0].toolbarModule.enableItems(['Export'], false)
                                                    document.getElementById("Grid").ej2_instances[0].toolbarModule.enableItems(['MarkAsPaid'], true)
                                                })
                                        }

                                        document.getElementById("btnABACancel").onclick = function (e) {
                                            dialog.hide()
                                            grid.hideSpinner()
                                        }

                                        document.getElementById("btnShowIssues").onclick = function (e) {

                                            var blob = new Blob([errorlist], { type: "text/plain;charset=utf-8" });
                                            const downloadUrl = URL.createObjectURL(blob)
                                            const a = document.createElement("a")
                                            a.href = downloadUrl
                                            a.download = "errors_batch_payments_export.aba"
                                            document.body.appendChild(a)
                                            a.click()
                                            grid.hideSpinner()
                                        }

                                    } else if (response.result == true) {

                                        var dialogProgress = document.getElementById("prompProgressDialog").ej2_instances[0]

                                        var dlgHeader = document.getElementById("prompProgressDialog_dialog-header")
                                        var dlgContent = document.getElementById("prompProgressDialog_dialog-content")

                                        dlgHeader.classList.add("prompt-dlg-header-template")
                                        dlgContent.classList.add("dlg-content-template")

                                        dialogProgress.show()

                                        document.getElementById("progressStatusOnly").innerHTML = "<span style='color:green;'><b>Status:</b>  Bank Account Selected...</span>";

                                        var count = 65;
                                        setInterval(function () {

                                            if (count <= 100) {
                                                document.getElementById('ProgOnly').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';
                                            } else {
                                                return;
                                            }
                                            count += 1;
                                        }, 100);

                                        fetch('/Xero/ExportToAbaFormat', {
                                            body: JSON.stringify(exportToABA),
                                            method: 'POST',
                                            headers: {
                                                'Content-Type': 'application/json; charset=utf-8'
                                            },
                                        }).then(response => response.blob())
                                            .then(response => {
                                                const blob = new Blob([response], { type: 'text/plain' })
                                                const downloadUrl = URL.createObjectURL(blob)
                                                const a = document.createElement("a")
                                                a.href = downloadUrl
                                                a.download = "batch_payments_export.aba"
                                                document.body.appendChild(a)
                                                a.click()
                                                grid.hideSpinner()
                                                dialogProgress.hide()
                                                disabledGridEditing = true
                                                document.getElementById("Grid").ej2_instances[0].toolbarModule.enableItems(['Export'], false)
                                                document.getElementById("Grid").ej2_instances[0].toolbarModule.enableItems(['MarkAsPaid'], true)
                                            })

                                    }
                                })
                        }
                        else {
                            tenantNotConnected()
                            dialogProgress.hide()
                        }
                    }
                    document.getElementById("btnCancel").onclick = function (e) {
                        promptDialog.hide()
                        grid.hideSpinner()
                    }
                }
                document.getElementById("btnPromptCancelTransactionDate").onclick = () => {
                    var dialog = document.getElementById("promptInfoDialogTransactionDate").ej2_instances[0]
                    grid.hideSpinner()
                    dialog.hide()
                }
            }
        }

        if (args.item.id === 'ExportCSV') {

            document.getElementById("progressStatus").innerHTML = "<span style='color:green;'><b>Status:</b>  Selecting export date...</span>";
            var count = 0;
            setInterval(function () {

                if (count <= 25) {
                    document.getElementById('Prog').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';
                } else {
                    return;
                }
                count += 1;
            }, 100);

            var grid = document.getElementById("Grid").ej2_instances[0]
            var dateTransaction = document.getElementById("dateTransaction").ej2_instances[0]
            grid.showSpinner()


            let selectedInvoices

            if (this.getSelectedRecords().length === 0) {
                selectedInvoices = objSelectedBills.Invoices
                isinsertRecordDB = "0";
            } else {
                selectedInvoices = this.getSelectedRecords();
                isinsertRecordDB = "1";
            }

            if (selectedInvoices.length === 0) {
                var message = "";

                if (selectedInvoices.length === 0) {
                    message += "* Please select invoices to export. <br/><br/>"
                }

                var dialog = document.getElementById("dialog").ej2_instances[0]
                document.getElementById("message").innerHTML = message

                var dlgHeader = document.getElementById("dialog_dialog-header")
                var dlgContent = document.getElementById("dialog_dialog-content")

                dlgHeader.classList.add("error-dlg-header-template")
                dlgContent.classList.add("dlg-content-template")

                dialog.show()
                grid.hideSpinner()
            } else {

                promptTransactionDate();
                document.getElementById("btnPromptOkayTransactionDate").onclick = () => {

                    if (dateTransaction.value === null) {

                        return
                    }

                    var dialog = document.getElementById("promptInfoDialogTransactionDate").ej2_instances[0]
                    dialog.hide()

                    var promptDialog = document.getElementById("promptDialog").ej2_instances[0]
                    document.getElementById("prompt").innerHTML = `Please confirm you’d like to proceed updating ${selectedInvoices.length} invoices in Xero to populate bank account in ABA file`

                    var dlgHeader = document.getElementById("promptDialog_dialog-header")
                    dlgHeader.classList.add("prompt-dlg-header-template")

                    var dlgContent = document.getElementById("promptDialog_dialog-content")
                    dlgContent.classList.add("dlg-content-template")

                    var bankDropDown = document.getElementById("bankAccountTypesDropDown").ej2_instances[0]
                    promptDialog.show()


                    document.getElementById("progressStatus2").innerHTML = "<span style='color:green;'><b>Status:</b>  Selecting Bank Account...</span>";


                    var count = 25;
                    setInterval(function () {

                        if (count <= 50) {
                            document.getElementById('Prog2').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';
                        } else {
                            return;
                        }
                        count += 1;
                    }, 100);


                    $.ajax({
                        url: '/Xero/GetBankAccountTypes',
                        type: 'GET',
                        headers: {
                            'Accept': 'application/json; charset=utf-8'
                        },
                        success: (result) => {
                            if (result.isError) {
                                operations.errorHandling(result)
                                return
                            }
                            else {

                                if (result.bankAccounts.length > 0)
                                    bankDropDown.dataSource = result.bankAccounts
                                else
                                    document.getElementById("errorContentBank").innerHTML = 'No bank account'
                            }
                        },
                        error: (error) => console.log(error)
                    })

                    document.getElementById("btnOkay").onclick = function (e) {

                        if (bankAccount === null || bankAccountName == null || bankAccount.length === 0 || bankAccountName.length === 0)
                            return

                        var dialogProgress = document.getElementById("prompProgressDialog").ej2_instances[0]

                        var dlgHeader = document.getElementById("prompProgressDialog_dialog-header")
                        var dlgContent = document.getElementById("prompProgressDialog_dialog-content")

                        dlgHeader.classList.add("prompt-dlg-header-template")
                        dlgContent.classList.add("dlg-content-template")

                        dialogProgress.show()

                        document.getElementById("progressStatusOnly").innerHTML = "<span style='color:green;'><b>Status:</b>  Bank Account Selected...</span>";

                        var count = 50;
                        setInterval(function () {

                            if (count <= 65) {
                                document.getElementById('ProgOnly').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';
                            } else {
                                return;
                            }
                            count += 1;
                        }, 100);


                        let errorCount = 0;
                        promptDialog.hide()

                        let exportToABA = {
                            bankAccountNumber: bankAccount,
                            bankAccountName: bankAccountName,
                            bankAccountCode: bankAccountCode,
                            errorCount: errorCount,
                            invoices: selectedInvoices,
                            isinsertRecordDB: isinsertRecordDB,
                            TransactionDate: formatDate(dateTransaction.value)
                        }

                        fetch('/Xero/ExportToAbaFormatChecking', {
                            body: JSON.stringify(exportToABA),
                            method: 'POST',
                            headers: {
                                'Content-Type': 'application/json; charset=utf-8'
                            },
                        }).then(response => response.json())
                            .then(response => {
                                if (response.result == false) {
                                    var dialog = document.getElementById("promptDialogABA").ej2_instances[0]

                                    var countryCode = '@ViewBag.orgCountryCode';
                                    var fileFormat = "ABA";
                                    var titleFormat = "";

                                    switch (countryCode) {
                                        case 'NZ':
                                            fileFormat = "a CSV";
                                            titleFormat = "CSV"
                                            break;
                                        case 'AU':
                                            fileFormat = "an ABA";
                                            titleFormat = "ABA"
                                            break;
                                        case 'GB':
                                            fileFormat = "a BACS";
                                            titleFormat = "BACS"
                                            break;
                                    }

                                    var errorlist = "";
                                    for (let i = 0; i < response.error.length; i++) {
                                        errorlist += response.error[i].error + "\n";
                                        errorCount++;
                                    }

                                    document.getElementById("fileFormat").innerHTML = titleFormat;
                                    document.getElementById("prompt").innerHTML = "You are about to generate " + fileFormat + " file containing " + response.countInvoice + " invoices. The file however contains " + response.countIssue + " errors.";
                                    var dlgHeader = document.getElementById("promptDialogABA_dialog-header")
                                    var dlgContent = document.getElementById("promptDialogABA_dialog-content")


                                    dlgHeader.classList.add("error-dlg-header-template")
                                    dlgContent.classList.add("dlg-content-template")

                                    if (selectedInvoices.length === response.error.length) {
                                        document.getElementById("btnABAOkay").removeAttribute("editable");
                                        document.getElementById("btnABAOkay").setAttribute("disabled", true);
                                    } else {
                                        document.getElementById("btnABAOkay").removeAttribute("disabled");
                                        document.getElementById("btnABAOkay").setAttribute("editable", true);
                                    }

                                    dialogProgress.hide()
                                    dialog.show()

                                    var count = 50;
                                    setInterval(function () {

                                        if (count <= 65) {
                                            document.getElementById('Prog3').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';
                                        } else {
                                            return;
                                        }
                                        count += 1;
                                    }, 100);

                                    document.getElementById("progressStatus3").innerHTML = "<span style='color:green;'><b>Status:</b>  Exporting data to CSV...</span>";
                                    grid.hideSpinner()
                                    promptDialog.hide()

                                    document.getElementById("btnABAOkay").onclick = function (e) {

                                        var count = 75;
                                        setInterval(function () {

                                            if (count <= 100) {
                                                document.getElementById('Prog3').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';
                                            } else {
                                                return;
                                            }
                                            count += 1;
                                        }, 200);

                                        document.getElementById("progressStatus3").innerHTML = "<span style='color:green;'><b>Status:</b>  Exporting data to CSV...</span>";

                                        let exportToABA2 = {
                                            bankAccountNumber: bankAccount,
                                            bankAccountName: bankAccountName,
                                            bankAccountCode: bankAccountCode,
                                            errorCount: errorCount,
                                            invoices: selectedInvoices,
                                            isinsertRecordDB: isinsertRecordDB,
                                            TransactionDate: formatDate(dateTransaction.value)
                                        }

                                        fetch('/Xero/ExportToCSVFormat', {
                                            body: JSON.stringify(exportToABA2),
                                            method: 'POST',
                                            headers: {
                                                'Content-Type': 'application/json; charset=utf-8'
                                            },
                                        }).then(response => response.blob())
                                            .then(response => {
                                                const blob = new Blob([response], { type: 'text/plain' })
                                                const downloadUrl = URL.createObjectURL(blob)
                                                const a = document.createElement("a")
                                                a.href = downloadUrl
                                                a.download = "batch_payments_export.csv"
                                                document.body.appendChild(a)
                                                dialog.hide()
                                                grid.hideSpinner()
                                                a.click()
                                                disabledGridEditing = true
                                                document.getElementById("Grid").ej2_instances[0].toolbarModule.enableItems(['ExportCSV'], false)
                                                document.getElementById("Grid").ej2_instances[0].toolbarModule.enableItems(['MarkAsPaid'], true)
                                            })
                                    }

                                    document.getElementById("btnABACancel").onclick = function (e) {
                                        dialog.hide()
                                        grid.hideSpinner()
                                    }

                                    document.getElementById("btnShowIssues").onclick = function (e) {

                                        var blob = new Blob([errorlist], { type: "text/plain;charset=utf-8" });
                                        const downloadUrl = URL.createObjectURL(blob)
                                        const a = document.createElement("a")
                                        a.href = downloadUrl
                                        a.download = "errors_batch_payments_export.txt"
                                        document.body.appendChild(a)
                                        a.click()

                                        grid.hideSpinner()
                                    }

                                } else if (response.result == true) {

                                    var count = 65;
                                    setInterval(function () {

                                        if (count <= 100) {
                                            document.getElementById('ProgOnly').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';
                                        } else {
                                            return;
                                        }
                                        count += 1;
                                    }, 100);

                                    fetch('/Xero/ExportToCSVFormat', {
                                        body: JSON.stringify(exportToABA),
                                        method: 'POST',
                                        headers: {
                                            'Content-Type': 'application/json; charset=utf-8'
                                        },
                                    }).then(response => response.blob())
                                        .then(response => {
                                            const blob = new Blob([response], { type: 'text/plain' })
                                            const downloadUrl = URL.createObjectURL(blob)
                                            const a = document.createElement("a")
                                            a.href = downloadUrl
                                            a.download = "batch_payments_export.csv"
                                            document.body.appendChild(a)
                                            a.click()
                                            dialogProgress.hide()
                                            grid.hideSpinner()
                                            disabledGridEditing = true
                                            document.getElementById("Grid").ej2_instances[0].toolbarModule.enableItems(['ExportCSV'], false)
                                            document.getElementById("Grid").ej2_instances[0].toolbarModule.enableItems(['MarkAsPaid'], true)
                                        })

                                }
                            })

                    }
                    document.getElementById("btnCancel").onclick = function (e) {
                        promptDialog.hide()
                        grid.hideSpinner()
                    }

                }

                document.getElementById("btnPromptCancelTransactionDate").onclick = () => {
                    var dialog = document.getElementById("promptInfoDialogTransactionDate").ej2_instances[0]
                    grid.hideSpinner()
                    dialog.hide()
                }
            }
        }

        if (args.item.id === "MarkAsPaid") {
            var grid = document.getElementById("Grid").ej2_instances[0]

            var dateTransaction = document.getElementById("dateTransaction2").ej2_instances[0]
            grid.showSpinner();
            var message = ""


            let selectedInvoices

            if (this.getSelectedRecords().length === 0) {
                selectedInvoices = objSelectedBills.Invoices
            } else {
                selectedInvoices = this.getSelectedRecords();
            }

            if (selectedInvoices.length === 0) {
                //grid.hideSpinner()
                if (selectedInvoices.length === 0) {
                    message += "* Please select bills to mark as paid. <br/><br/>"
                }

                var dialog = document.getElementById("dialog").ej2_instances[0]
                document.getElementById("message").innerHTML = message
                var dlgHeader = document.getElementById("dialog_dialog-header")
                var dlgContent = document.getElementById("dialog_dialog-content")

                dlgHeader.classList.add("error-dlg-header-template")
                dlgContent.classList.add("dlg-content-template")
                dialog.show()
                grid.hideSpinner()

            } else {
                if (selectedInvoices.filter(i => i.hasEmailAddress == false).length > 0) {

                    var NumOfInvoice;
                    var NumOfIssues;

                    promptEmailAddressValidation(selectedInvoices)

                    document.getElementById("progressStatus10").style.display = "block";
                    document.getElementById("Prog10").style.display = "block";

                    var count = 0;
                    let myInterval = setInterval(function () {

                        if (count <= 20) {
                            document.getElementById('Prog10').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';
                        } else {
                            clearInterval(myInterval)
                            return;
                        }
                        count += 1;
                    }, 100);

                    document.getElementById("progressStatus10").innerHTML = "<span style='color:green;'><b>Status:</b> Verifying Email Address...</span>";

                    document.getElementById("btnMarkAsPaidCancel").onclick = function (e) {
                        var dialog = document.getElementById("promptDialogMarkAsPaid").ej2_instances[0]
                        dialog.hide()
                    }


                    document.getElementById("btnMarkAsPaidOkay").onclick = () => {

                        var dialog = document.getElementById("promptDialogMarkAsPaid").ej2_instances[0]
                        dialog.hide()
                        promptTransactionDate2();

                        var count = 20;
                        let myInterval = setInterval(function () {

                            if (count <= 30) {
                                document.getElementById('Prog5').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';
                            } else {
                                clearInterval(myInterval)
                                return;
                            }
                            count += 1;
                        }, 100);

                        document.getElementById("progressStatus5").innerHTML = "<span style='color:green;'><b>Status:</b>  Selecting export date...</span>";

                        document.getElementById("btnPromptOkayTransactionDate2").onclick = () => {

                            if (dateTransaction.value === null) {
                                return
                            }

                            var dialog = document.getElementById("promptInfoDialogTransactionDate2").ej2_instances[0]
                            dialog.hide()

                            var promptDialog = document.getElementById("promptDialog").ej2_instances[0]
                            document.getElementById("prompt").innerHTML = `Please confirm you’d like to proceed updating ${selectedInvoices.length} invoices in Xero as PAID`

                            var dlgHeader = document.getElementById("promptDialog_dialog-header")
                            dlgHeader.classList.add("prompt-dlg-header-template")

                            var dlgContent = document.getElementById("promptDialog_dialog-content")
                            dlgContent.classList.add("dlg-content-template")

                            var bankDropDown = document.getElementById("bankAccountTypesDropDown").ej2_instances[0]
                            document.getElementById("errorContentBank").innerHTML = ''

                            promptDialog.show()

                            var count = 30;
                            let myInterval = setInterval(function () {

                                if (count <= 50) {
                                    document.getElementById('Prog2').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';
                                } else {
                                    clearInterval(myInterval)
                                    return;
                                }
                                count += 1;
                            }, 100);

                            document.getElementById("progressStatus2").innerHTML = "<span style='color:green;'><b>Status:</b> Selecting Bank Account...</span>";

                            $.ajax({
                                url: '/Xero/GetBankAccountTypes',
                                type: 'GET',
                                headers: {
                                    'Accept': 'application/json; charset=utf-8'
                                },
                                success: (result) => {

                                    if (result.isError) {
                                        operations.errorHandling(result)
                                        return
                                    }
                                    else {

                                        if (result.bankAccounts.length > 0)
                                            bankDropDown.dataSource = result.bankAccounts
                                        else
                                            document.getElementById("errorContentBank").innerHTML = 'No bank account'
                                    }
                                },
                                error: (error) => {
                                    document.getElementById("errorContentBank").innerHTML = [error.statusText, error.responseText].join(' ')
                                }
                            })


                            document.getElementById("btnOkay").onclick = function (e) {

                                if (bankAccount === null || bankAccountName == null || bankAccount.length === 0 || bankAccountName.length === 0)
                                    return

                                var dialogProgress = document.getElementById("prompProgressDialog").ej2_instances[0]

                                var dlgHeader = document.getElementById("prompProgressDialog_dialog-header")
                                var dlgContent = document.getElementById("prompProgressDialog_dialog-content")

                                dlgHeader.classList.add("prompt-dlg-header-template")
                                dlgContent.classList.add("dlg-content-template")

                                dialogProgress.show()

                                document.getElementById("progressStatusOnly").innerHTML = "<span style='color:green;'><b>Status:</b> Marking invoices as paid...</span>";

                                var count = 50;
                                let myInterval = setInterval(function () {

                                    if (count <= 65) {
                                        document.getElementById('ProgOnly').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';
                                    } else {
                                        clearInterval(myInterval)
                                        return;
                                    }
                                    count += 1;
                                }, 100);

                                promptDialog.hide()
                                let payload = {
                                    bankAccountNumber: bankAccount,
                                    bankAccountCode: bankAccountCode,
                                    invoices: selectedInvoices,
                                    TransactionDate: formatDate(dateTransaction.value)
                                }


                                if (CheckIfConnectedToXero()) {

                                    $.ajax({
                                        url: '/Xero/MarkBillsAsPaid',
                                        type: 'POST',
                                        data: JSON.stringify(payload),
                                        beforeSend: function () {
                                            promptDialog.hide()
                                            //grid.showSpinner()
                                        },
                                        headers: {
                                            'Content-Type': 'application/json; charset=utf-8',
                                            'Accept': 'application/json; charset=utf-8'
                                        },
                                        success: (result) => {
                                            if (result.isError) {
                                                dialogProgress.hide()
                                                document.getElementById("Grid").ej2_instances[0].hideSpinner()
                                                operations.errorHandling(result)


                                            } else {
                                                disabledGridEditing = true
                                                markAsPaidResult = result
                                                document.getElementById("Grid").ej2_instances[0].toolbarModule.enableItems(['MarkAsPaid'], false)
                                                switch (countryCode) {
                                                    case "AU":
                                                        document.getElementById("Grid").ej2_instances[0].toolbarModule.enableItems(['Export'], false)
                                                        break;
                                                    case "NZ":
                                                        document.getElementById("Grid").ej2_instances[0].toolbarModule.enableItems(['ExportCSV'], false)
                                                        break;
                                                    case "GB":
                                                        document.getElementById("Grid").ej2_instances[0].toolbarModule.enableItems(['ExportToBacs'], false)
                                                        break;
                                                    default:
                                                }
                                                document.getElementById("Grid").ej2_instances[0].toolbarModule.enableItems(['SendRemittances', 'ExportBatchPaymentReport'], true)

                                                var numberOfMinutesToProcess = (markAsPaidResult.invoices._Payments.length / 50)
                                                var progressStatusMessageLessThan50 = "<span style='color:green;'><b>Status:</b>  Creating invoices history/note. <br/> <br/> Please wait for a few minutes...</span>"
                                                var progressStatusMessageMoreThanEqual50 = "<span style='color:green;'><b>Status:</b>  Creating invoices history/note. <br/> <br/> Please wait for " + Math.ceil(numberOfMinutesToProcess) + " minutes...</span>"
                                                document.getElementById("progressStatusOnly").innerHTML = (numberOfMinutesToProcess > 1) ? progressStatusMessageMoreThanEqual50 : progressStatusMessageLessThan50;

                                                var count = 65

                                                let myInterval = setInterval(function () {

                                                    if (count <= 80) {
                                                        document.getElementById('ProgOnly').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';
                                                    } else {
                                                        //dialogProgress.hide();
                                                        operations.UpdatePaymentHistory(result, dialogProgress);
                                                        //operations.onGetBills()
                                                        clearInterval(myInterval)
                                                        return;
                                                    }
                                                    count += 1;
                                                }, 100)
                                            }

                                        },
                                        error: (error) => {
                                            let errorDetail = {
                                                invoiceNumber: "",
                                                errors: []
                                            }
                                            let errors = {
                                                errorDetail: []
                                            }

                                            let payload = operations.extractJson(error.responseText)
                                            payload.Elements.forEach((item) => {
                                                errors.errorDetail.push({
                                                    invoiceNumber: item.InvoiceNumber,
                                                    errors: item.ValidationErrors
                                                })
                                            })

                                            operations.errorFormat(errors)
                                            grid.hideSpinner()
                                        }
                                    })
                                } else {
                                    dialogProgress.hide();
                                    tenantNotConnected()
                                }
                            }

                            document.getElementById("btnCancel").onclick = function (e) {
                                promptDialog.hide()
                                grid.hideSpinner()
                            }

                        }

                        document.getElementById("btnPromptCancelTransactionDate2").onclick = () => {
                            var dialog = document.getElementById("promptInfoDialogTransactionDate2").ej2_instances[0]
                            dialog.hide()
                        }
                    }


                }
                else {

                    promptTransactionDate2();

                    var count = 0;
                    setInterval(function () {

                        if (count <= 20) {
                            document.getElementById('Prog5').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';
                        } else {
                            return;
                        }
                        count += 1;
                    }, 100);

                    document.getElementById("progressStatus5").innerHTML = "<span style='color:green;'><b>Status:</b>  Selecting export date...</span>";

                    document.getElementById("btnPromptOkayTransactionDate2").onclick = () => {

                        if (dateTransaction.value === null) {

                            return
                        }

                        var dialog = document.getElementById("promptInfoDialogTransactionDate2").ej2_instances[0]
                        dialog.hide()

                        var promptDialog = document.getElementById("promptDialog").ej2_instances[0]
                        document.getElementById("prompt").innerHTML = `Please confirm you’d like to proceed updating ${selectedInvoices.length} invoices in Xero as PAID`

                        var dlgHeader = document.getElementById("promptDialog_dialog-header")
                        dlgHeader.classList.add("prompt-dlg-header-template")

                        var dlgContent = document.getElementById("promptDialog_dialog-content")
                        dlgContent.classList.add("dlg-content-template")

                        var bankDropDown = document.getElementById("bankAccountTypesDropDown").ej2_instances[0]
                        document.getElementById("errorContentBank").innerHTML = ''

                        promptDialog.show()

                        var count = 20;
                        setInterval(function () {

                            if (count <= 30) {
                                document.getElementById('Prog2').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';
                            } else {
                                return;
                            }
                            count += 1;
                        }, 100);

                        document.getElementById("progressStatus2").innerHTML = "<span style='color:green;'><b>Status:</b> Selecting Bank Account...</span>";

                        $.ajax({
                            url: '/Xero/GetBankAccountTypes',
                            type: 'GET',
                            headers: {
                                'Accept': 'application/json; charset=utf-8'
                            },
                            success: (result) => {

                                if (result.sessionTimeOut)
                                    window.location = '/Home/Index'

                                if (result.IsError) {
                                    document.getElementById("errorContentBank").innerHTML = 'No bank account'
                                    return
                                }
                                else {

                                    if (result.bankAccounts.length > 0)
                                        bankDropDown.dataSource = result.bankAccounts
                                    else
                                        document.getElementById("errorContentBank").innerHTML = 'No bank account'
                                }
                            },
                            error: (error) => {
                                document.getElementById("errorContentBank").innerHTML = [error.statusText, error.responseText].join(' ')
                            }
                        })

                        document.getElementById("btnOkay").onclick = function (e) {

                            if (bankAccount === null || bankAccountName == null || bankAccount.length === 0 || bankAccountName.length === 0)
                                return

                            var dialogProgress = document.getElementById("prompProgressDialog").ej2_instances[0]

                            var dlgHeader = document.getElementById("prompProgressDialog_dialog-header")
                            var dlgContent = document.getElementById("prompProgressDialog_dialog-content")

                            dlgHeader.classList.add("prompt-dlg-header-template")
                            dlgContent.classList.add("dlg-content-template")

                            dialogProgress.show()

                            document.getElementById("progressStatusOnly").innerHTML = "<span style='color:green;'><b>Status:</b>  Marking invoices as paid...</span>";

                            var count = 30;
                            setInterval(function () {

                                if (count <= 50) {
                                    document.getElementById('ProgOnly').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';
                                } else {
                                    return;
                                }
                                count += 1;
                            }, 100);


                            promptDialog.hide()
                            let payload = {
                                bankAccountNumber: bankAccount,
                                bankAccountCode: bankAccountCode,
                                invoices: selectedInvoices,
                                TransactionDate: formatDate(dateTransaction.value)
                            }

                            if (CheckIfConnectedToXero()) {
                                $.ajax({
                                    url: '/Xero/MarkBillsAsPaid',
                                    type: 'POST',
                                    data: JSON.stringify(payload),
                                    beforeSend: function () {
                                        promptDialog.hide()
                                        //grid.showSpinner()
                                    },
                                    headers: {
                                        'Content-Type': 'application/json; charset=utf-8',
                                        'Accept': 'application/json; charset=utf-8'
                                    },
                                    success: (result) => {
                                        if (result.isError) {
                                            dialogProgress.hide()
                                            operations.errorHandling(result)

                                        } else {
                                            disabledGridEditing = true
                                            markAsPaidResult = result
                                            document.getElementById("Grid").ej2_instances[0].toolbarModule.enableItems(['MarkAsPaid'], false)
                                            switch (countryCode) {
                                                case "AU":
                                                    document.getElementById("Grid").ej2_instances[0].toolbarModule.enableItems(['Export'], false)
                                                    break;
                                                case "NZ":
                                                    document.getElementById("Grid").ej2_instances[0].toolbarModule.enableItems(['ExportCSV'], false)
                                                    break;
                                                case "GB":
                                                    document.getElementById("Grid").ej2_instances[0].toolbarModule.enableItems(['ExportToBacs'], false)
                                                    break;
                                                default:
                                            }
                                            document.getElementById("Grid").ej2_instances[0].toolbarModule.enableItems(['SendRemittances', 'ExportBatchPaymentReport'], true)
                                            var numberOfMinutesToProcess = (markAsPaidResult.invoices._Payments.length / 50)
                                            var progressStatusMessageLessThan50 = "<span style='color:green;'><b>Status:</b>  Creating invoices history/note. <br/> <br/> Please wait for a few minutes...</span>"
                                            var progressStatusMessageMoreThanEqual50 = "<span style='color:green;'><b>Status:</b>  Creating invoices history/note. <br/> <br/> Please wait for " + Math.ceil(numberOfMinutesToProcess) + " minutes...</span>"
                                            document.getElementById("progressStatusOnly").innerHTML = (numberOfMinutesToProcess > 1) ? progressStatusMessageMoreThanEqual50 : progressStatusMessageLessThan50;
                                            var count = 50;
                                            let myInterval = setInterval(function () {

                                                if (count <= 65) {
                                                    document.getElementById('ProgOnly').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';
                                                } else {
                                                    // document.getElementById("prompProgressDialog").ej2_instances[0].hide()
                                                    // grid.hideSpinner()
                                                    //dialogProgress.hide()
                                                    operations.UpdatePaymentHistory(result, dialogProgress);
                                                    clearInterval(myInterval)
                                                    //operations.onGetBills()
                                                    return;
                                                }
                                                count += 1;
                                            }, 100);
                                        }
                                    },
                                    error: (error) => {
                                        let errorDetail = {
                                            invoiceNumber: "",
                                            errors: []
                                        }
                                        let errors = {
                                            errorDetail: []
                                        }

                                        let payload = operations.extractJson(error.responseText)
                                        payload.Elements.forEach((item) => {
                                            errors.errorDetail.push({
                                                invoiceNumber: item.InvoiceNumber,
                                                errors: item.ValidationErrors
                                            })
                                        })

                                        operations.errorFormat(errors)
                                        grid.hideSpinner()
                                    }
                                })
                            } else {
                                dialogProgress.hide();
                                tenantNotConnected()
                            }

                        }
                        document.getElementById("btnCancel").onclick = function (e) {
                            promptDialog.hide()
                            grid.hideSpinner()
                        }

                    }
                    document.getElementById("btnPromptCancelTransactionDate2").onclick = () => {
                        var dialog = document.getElementById("promptInfoDialogTransactionDate2").ej2_instances[0]
                        dialog.hide()
                    }
                }
            }
            grid.hideSpinner();
        }

        if (args.item.id == "SendRemittances") {

            let selectedInvoices

            if (this.getSelectedRecords().length === 0) {
                selectedInvoices = objSelectedBills.Invoices
            } else {
                selectedInvoices = this.getSelectedRecords();
            }

            var dialog = document.getElementById("promptInfoDialog").ej2_instances[0]
            document.getElementById("promptContent").innerHTML = "Do you want to send Remittance Advice?"

            var dlgHeader = document.getElementById("promptInfoDialog_dialog-header")
            dlgHeader.classList.add("prompt-dlg-header-template")

            var dlgContent = document.getElementById("promptInfoDialog_dialog-content")
            dlgContent.classList.add("dlg-content-template")
            document.getElementById("progressStatus4").style.display = "initial";
            document.getElementById("Prog4").style.display = "initial";
            document.getElementById("btnPromptOkay").style.display = "initial";
            document.getElementById("btnPromptCancel").style.display = "initial";
            dialog.show()

            var count = 0;
            var myInterval = setInterval(function () {

                if (count <= 20) {
                    document.getElementById('Prog4').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';
                } else {
                    clearInterval(myInterval)
                    return;
                }
                count += 1;
            }, 100);

            document.getElementById("progressStatus4").innerHTML = "<span style='color:green;'><b>Status:</b> Sending Remmittance...</span>";

            document.getElementById("btnPromptOkay").onclick = () => {
                //alert('testeet')
                //var dialogProgress = document.getElementById("prompProgressDialog").ej2_instances[0]
                //var dlgHeader = document.getElementById("prompProgressDialog_dialog-header")
                //var dlgContent = document.getElementById("prompProgressDialog_dialog-content")

                //dlgHeader.classList.add("prompt-dlg-header-template")
                //dlgContent.classList.add("dlg-content-template")
                ////dialog.hide()
                //dialogProgress.show()

                //document.getElementById("progressStatusOnly").innerHTML = "<span style='color:green;'><b>Status:</b>  Sending Remmittance...</span>";
                document.getElementById("promptContent").innerHTML = ""
                document.getElementById("btnPromptOkay").style.display = "none"
                document.getElementById("btnPromptCancel").style.display = "none"
                var count = 20;
                var myInterval = setInterval(function () {

                    if (count <= 80) {
                        document.getElementById('Prog4').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';
                    } else {
                        clearInterval(myInterval)
                        return;
                    }
                    count += 1;

                }, 100);

                operations.onSendRemittanceAdvice(selectedInvoices, markAsPaidResult)
            }

            //document.getElementById("btnPromptCancel").onclick = () => {
            //    document.getElementById("promptInfoDialog").ej2_instances[0].hide()
            //    document.getElementById("prompProgressDialog").ej2_instances[0].hide()
            //    document.getElementById("Grid").ej2_instances[0].hideSpinner()
            //}
        }

        if (args.item.id == "ExportBatchPaymentReport") {
            let selectedInvoices

            if (this.getSelectedRecords().length === 0) {
                selectedInvoices = objSelectedBills.Invoices
            } else {
                selectedInvoices = this.getSelectedRecords();
            }
            operations.ExportBatchPaymentReport(selectedInvoices, markAsPaidResult);
            //operations.onGetBills();
        }
        if (args.item.id === "Refresh") {

            if (disabledGridEditing)
                return

            document.getElementById("selectedBills").innerHTML = "0"
            document.getElementById("total").innerHTML = "0"

            var grid = document.getElementById("Grid").ej2_instances[0]
            grid.showSpinner()

            let payload = {
                page: 1
            }

            if (CheckIfConnectedToXero()) {

                var ajax = new ej.base.Ajax({
                    url: '/Xero/FilterBills',
                    type: 'POST',
                    data: JSON.stringify(payload),
                    contentType: 'application/json',
                    accept: 'application/json'
                })
                ajax.send()
                ajax.onSuccess = function (data) {
                    var obj = JSON.parse(data)
                    if (obj.isError) {
                        operations.errorHandling(obj)
                        return
                    }
                    grid.dataSource = obj.invoices
                    grid.hideSpinner()
                    grid.clearFiltering()
                    grid.clearCellSelection()
                    grid.clearRowSelection()
                    grid.clearSelection()
                    grid.selectionModule.deSelectedData = []
                    grid.selectionModule.persistSelectedData = []
                    total = 0
                }

            }
            else
                tenantNotConnected()

        }
        if (args.item.id === "SortColumns") {
            var gridSorting = document.getElementById("GridSorting").ej2_instances[0]
            gridSorting.dataSource = []
            dialogObj.show()
        }
        if (args.item.id === "ClearSortColumns") {
            var grid = document.getElementById("Grid").ej2_instances[0]
            grid.clearSorting();
        }
        if (args.item.id === 'ExportToBacs') {

            document.getElementById("progressStatus").innerHTML = "<span style='color:green;'><b>Status:</b>  Selecting export date...</span>";
            var count = 0;
            setInterval(function () {

                if (count <= 25) {
                    document.getElementById('Prog').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';
                } else {
                    return;
                }
                count += 1;
            }, 100);


            var grid = document.getElementById("Grid").ej2_instances[0]
            var dateTransaction = document.getElementById("dateTransaction").ej2_instances[0]
            grid.showSpinner()


            let selectedInvoices

            if (this.getSelectedRecords().length === 0) {
                selectedInvoices = objSelectedBills.Invoices
                isinsertRecordDB = "0";
            } else {
                selectedInvoices = this.getSelectedRecords();
                isinsertRecordDB = "1";
            }


            if (selectedInvoices.length === 0) {
                var message = "";

                if (selectedInvoices.length === 0) {
                    message += "* Please select invoices to export. <br/><br/>"
                }

                var dialog = document.getElementById("dialog").ej2_instances[0]
                document.getElementById("message").innerHTML = message

                var dlgHeader = document.getElementById("dialog_dialog-header")
                var dlgContent = document.getElementById("dialog_dialog-content")

                dlgHeader.classList.add("error-dlg-header-template")
                dlgContent.classList.add("dlg-content-template")

                dialog.show()
                grid.hideSpinner()
            } else {

                promptTransactionDate();
                document.getElementById("btnPromptOkayTransactionDate").onclick = () => {

                    if (dateTransaction.value === null) {

                        return
                    }

                    var dialog = document.getElementById("promptInfoDialogTransactionDate").ej2_instances[0]
                    dialog.hide()

                    var promptDialog = document.getElementById("promptDialog").ej2_instances[0]
                    document.getElementById("prompt").innerHTML = `Please confirm you’d like to proceed updating ${selectedInvoices.length} invoices in Xero to populate bank account in BACS file`

                    var dlgHeader = document.getElementById("promptDialog_dialog-header")
                    dlgHeader.classList.add("prompt-dlg-header-template")

                    var dlgContent = document.getElementById("promptDialog_dialog-content")
                    dlgContent.classList.add("dlg-content-template")

                    var bankDropDown = document.getElementById("bankAccountTypesDropDown").ej2_instances[0]

                    promptDialog.show()

                    document.getElementById("progressStatus2").innerHTML = "<span style='color:green;'><b>Status:</b>  Selecting Bank Account...</span>";

                    var count = 25;
                    setInterval(function () {
                        if (count <= 50) {
                            document.getElementById('Prog2').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';
                        } else {
                            return;
                        }
                        count += 1;
                    }, 100);

                    $.ajax({
                        url: '/Xero/GetBankAccountTypes',
                        type: 'GET',
                        headers: {
                            'Accept': 'application/json; charset=utf-8'
                        },
                        success: (result) => {
                            if (result.isError) {
                                operations.errorHandling(result)
                                return
                            }
                            else {

                                if (result.bankAccounts.length > 0)
                                    bankDropDown.dataSource = result.bankAccounts
                                else
                                    document.getElementById("errorContentBank").innerHTML = 'No bank account'
                            }
                        },
                        error: (error) => console.log(error)
                    })

                    document.getElementById("btnOkay").onclick = function (e) {
                        if (bankAccount === null || bankAccountName == null || bankAccount.length === 0 || bankAccountName.length === 0)
                            return

                        var dialogProgress = document.getElementById("prompProgressDialog").ej2_instances[0]
                        var dlgHeader = document.getElementById("prompProgressDialog_dialog-header")
                        var dlgContent = document.getElementById("prompProgressDialog_dialog-content")

                        dlgHeader.classList.add("prompt-dlg-header-template")
                        dlgContent.classList.add("dlg-content-template")
                        dialogProgress.show()
                        document.getElementById("progressStatusOnly").innerHTML = "<span style='color:green;'><b>Status:</b>  Bank Account Selected...</span>";

                        var count = 50;
                        setInterval(function () {

                            if (count <= 65) {
                                document.getElementById('ProgOnly').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';
                            } else {
                                return;
                            }
                            count += 1;
                        }, 100);

                        let errorCount = 0;
                        promptDialog.hide()
                        //TODO
                        let exportToABA = {
                            bankAccountNumber: bankAccount,
                            bankAccountName: bankAccountName,
                            bankAccountCode: bankAccountCode,
                            errorCount: errorCount,
                            invoices: selectedInvoices,
                            isinsertRecordDB: isinsertRecordDB,
                            TransactionDate: formatDate(dateTransaction.value)
                        }

                        fetch('/Xero/ExportToAbaFormatChecking', {
                            body: JSON.stringify(exportToABA),
                            method: 'POST',
                            headers: {
                                'Content-Type': 'application/json; charset=utf-8'
                            },
                        }).then(response => response.json())
                            .then(response => {
                                if (response.result == false) {
                                    var dialog = document.getElementById("promptDialogABA").ej2_instances[0]

                                    var errorlist = "";
                                    for (let i = 0; i < response.error.length; i++) {
                                        errorlist += response.error[i].error + "\n";
                                        errorCount++;
                                    }

                                    document.getElementById("prompt").innerHTML = "The following " + response.countInvoice + " invoices exported to bacs file has " + response.countIssue + " issues.";
                                    var dlgHeader = document.getElementById("promptDialogABA_dialog-header")
                                    var dlgContent = document.getElementById("promptDialogABA_dialog-content")
                                    document.getElementById("promptDialogABA_title").innerHTML = "Issues in Bacs";

                                    dlgHeader.classList.add("error-dlg-header-template")
                                    dlgContent.classList.add("dlg-content-template")

                                    if (selectedInvoices.length === response.error.length) {
                                        document.getElementById("btnABAOkay").removeAttribute("editable");
                                        document.getElementById("btnABAOkay").setAttribute("disabled", true);
                                    } else {
                                        document.getElementById("btnABAOkay").removeAttribute("disabled");
                                        document.getElementById("btnABAOkay").setAttribute("editable", true);
                                    }

                                    dialogProgress.hide()
                                    dialog.show()
                                    var count = 50;
                                    setInterval(function () {

                                        if (count <= 65) {
                                            document.getElementById('Prog3').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';
                                        } else {
                                            return;
                                        }
                                        count += 1;
                                    }, 100);

                                    document.getElementById("progressStatus3").innerHTML = "<span style='color:green;'><b>Status:</b>  Exporting data to CSV...</span>";

                                    grid.hideSpinner()
                                    promptDialog.hide()

                                    document.getElementById("btnABAOkay").onclick = function (e) {

                                        let exportToABA2 = {
                                            bankAccountNumber: bankAccount,
                                            bankAccountName: bankAccountName,
                                            bankAccountCode: bankAccountCode,
                                            errorCount: errorCount,
                                            invoices: selectedInvoices,
                                            isinsertRecordDB: isinsertRecordDB,
                                            TransactionDate: formatDate(dateTransaction.value)
                                        }

                                        fetch('/Xero/ExportToBacsFormat', {
                                            body: JSON.stringify(exportToABA2),
                                            method: 'POST',
                                            headers: {
                                                'Content-Type': 'application/json; charset=utf-8'
                                            },
                                        }).then(response => response.blob())
                                            .then(response => {
                                                const blob = new Blob([response], { type: 'text/plain' })
                                                const downloadUrl = URL.createObjectURL(blob)
                                                const a = document.createElement("a")
                                                a.href = downloadUrl
                                                a.download = "batch_payments_export.BAC"
                                                document.body.appendChild(a)
                                                a.click()

                                                var count = 75;
                                                setInterval(function () {

                                                    if (count <= 100) {
                                                        document.getElementById('Prog3').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';
                                                    } else {
                                                        dialog.hide()
                                                        grid.hideSpinner()
                                                    }
                                                    count += 1;
                                                }, 100);

                                                disabledGridEditing = true
                                                document.getElementById("Grid").ej2_instances[0].toolbarModule.enableItems(['ExportToBacs'], false)
                                                document.getElementById("Grid").ej2_instances[0].toolbarModule.enableItems(['MarkAsPaid'], true)
                                                document.getElementById("progressStatus3").innerHTML = "<span style='color:green;'><b>Status:</b>  Exporting data to BACS...</span>";
                                            })
                                    }

                                    document.getElementById("btnABACancel").onclick = function (e) {
                                        dialog.hide()
                                        grid.hideSpinner()
                                    }

                                    document.getElementById("btnShowIssues").onclick = function (e) {

                                        var blob = new Blob([errorlist], { type: "text/plain;charset=utf-8" });
                                        const downloadUrl = URL.createObjectURL(blob)
                                        const a = document.createElement("a")
                                        a.href = downloadUrl
                                        a.download = "errors_batch_payments_export.BAC"
                                        document.body.appendChild(a)
                                        a.click()

                                        grid.hideSpinner()
                                    }

                                } else if (response.result == true) {

                                    var count = 65;
                                    setInterval(function () {
                                        if (count <= 100) {
                                            document.getElementById('ProgOnly').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';

                                        } else {
                                            dialogProgress.hide()
                                            dialog.hide()
                                            grid.hideSpinner()
                                        }
                                        count += 1;
                                    }, 100);

                                    fetch('/Xero/ExportToBacsFormat', {
                                        body: JSON.stringify(exportToABA),
                                        method: 'POST',
                                        headers: {
                                            'Content-Type': 'application/json; charset=utf-8'
                                        },
                                    }).then(response => response.blob())
                                        .then(response => {
                                            const blob = new Blob([response], { type: 'text/plain' })
                                            const downloadUrl = URL.createObjectURL(blob)
                                            const a = document.createElement("a")
                                            a.href = downloadUrl
                                            a.download = "batch_payments_export.BAC"
                                            document.body.appendChild(a)
                                            a.click()
                                            grid.hideSpinner()
                                            disabledGridEditing = true
                                            document.getElementById("Grid").ej2_instances[0].toolbarModule.enableItems(['ExportToBacs'], false)
                                            document.getElementById("Grid").ej2_instances[0].toolbarModule.enableItems(['MarkAsPaid'], true)

                                        })

                                }
                            })

                    }
                    document.getElementById("btnCancel").onclick = function (e) {
                        promptDialog.hide()
                        grid.hideSpinner()
                    }

                }

                document.getElementById("btnPromptCancelTransactionDate").onclick = () => {
                    document.getElementById("progressStatusOnly").hide();
                    dialog.hide()
                }
            }
        }
    },
    onFilterBillByDate: function (args) {
        let status = document.getElementById("billsStatusFilterDropDown").ej2_instances[0]

        var grid = document.getElementById("Grid").ej2_instances[0]
        grid.showSpinner()

        let payload = {
            status: status.itemData,
            startDate: new Date(args.startDate),
            endDate: new Date(args.endDate)
        }

        if (CheckIfConnectedToXero()) {
            var ajax = new ej.base.Ajax({
                url: '/Xero/FilterBills',
                type: 'POST',
                data: JSON.stringify(payload),
                contentType: 'application/json',
                accept: 'application/json'
            })
            ajax.send()
            ajax.onSuccess = function (data) {
                var obj = JSON.parse(data)
                if (obj.isError) {
                    operations.errorHandling(obj)
                    return
                }
                grid.dataSource = obj.invoices
                grid.hideSpinner()
            }
        }
        else
            tenantNotConnected()

    },
    onGridLoad: function (args) {
        var grid = document.getElementById("Grid").ej2_instances[0]
        grid.showSpinner()
        var pager = document.getElementsByClassName('e-gridpager');
        var topElement;
        if (this.toolbar) {
            topElement = document.getElementsByClassName('e-toolbar');
        } else {
            topElement = document.getElementsByClassName('e-gridheader');
        }
        topElement[0].before(pager[0]);
        grid.hideSpinner()
    },
    extractJson: function (str) {
        var firstOpen, firstClose, candidate;
        firstOpen = str.indexOf('{', firstOpen + 1);
        do {
            firstClose = str.lastIndexOf('}');
            if (firstClose <= firstOpen) {
                return null;
            }
            do {
                candidate = str.substring(firstOpen, firstClose + 1);
                try {
                    return JSON.parse(candidate);
                    //return [res, firstOpen, firstClose + 1];
                }
                catch (e) {
                    console.log('...failed');
                }
                firstClose = str.substr(0, firstClose).lastIndexOf('}');
            } while (firstClose > firstOpen);
            firstOpen = str.indexOf('{', firstOpen + 1);
        } while (firstOpen != -1);
    },
    errorFormat: function (errors) {
        var dialog = document.getElementById("errorDialog").ej2_instances[0]
        var errorContent = document.getElementById("errorContent")
        var dlgHeader = document.getElementById("errorDialog_dialog-header")
        var dlgContent = document.getElementById("errorDialog_dialog-content")

        if (errorContent.hasChildNodes()) {
            while (errorContent.firstChild) {
                errorContent.removeChild(errorContent.firstChild)
            }
        }

        dlgHeader.classList.add("error-dlg-header-template")
        dlgContent.classList.add("dlg-content-template")

        errors.errorDetail.forEach((error, index) => {
            let header = document.createElement("h4")
            let ul = document.createElement("ul")

            header.innerText = error.invoiceNumber == "" ? "<No-Invoice-Number>" : error.invoiceNumber
            error.errors.forEach((msg) => {
                let li = document.createElement("li")
                li.innerText = msg.Message
                ul.append(li)
            })

            errorContent.append(header, ul)
        })

        document.getElementById("btnCloseErrorDialog").onclick = function (e) {
            dialog.hide()
            grid.hideSpinner()
        }
        dialog.show()
    },
    onModalLoad: function () {
        dialogObj = this
        dialogObj.hide()
    },
    onRowDeselecting: function (args) {
        if (disabledGridEditing)
            args.cancel = true
    },
    onBeginEdit: function (args) {
        if (disabledGridEditing)
            args.cancel = true
    },
    onRowSelecting: function (args) {
        if (disabledGridEditing)
            args.cancel = true
    },
    onSelectBills: function (args) {
        if (args.isHeaderCheckboxClicked === true && args.data.length > 0 && args.isInteracted) {
            billsSelected = this.getSelectedRecords()

            count = billsSelected.length
            document.getElementById("selectedBills").innerHTML = count
            total = 0
            billsSelected.forEach((item) => {
                total += item.amountDueEditable
            })

            document.getElementById("total").innerHTML = "$" + total.toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,')
        } else if (args.isHeaderCheckboxClicked === false && args.isInteracted) {
            billsSelected = this.getSelectedRecords()

            count = billsSelected.length
            document.getElementById("selectedBills").innerHTML = count
            total = 0
            //total += args.data.amountDue

            billsSelected.forEach((item) => {
                total += item.amountDueEditable
            })
            document.getElementById("total").innerHTML = "$" + total.toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,')
        }
    },
    onDeselectBills: function (args) {
        billsSelected = this.getSelectedRecords()

        if (args.isHeaderCheckboxClicked === true && billsSelected.length == 0 && args.isInteracted) {
            count = 0
            total = 0

            document.getElementById("selectedBills").innerHTML = count
            document.getElementById("total").innerHTML = "$" + total
        } else if (args.isHeaderCheckboxClicked === false && args.isInteracted) {
            document.getElementById("selectedBills").innerHTML = billsSelected.length

            total -= args.data.amountDueEditable
            total = isNaN(total) ? 0 : total
            document.getElementById("total").innerHTML = "$" + total.toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,')
        }
    },
    onSelectBank: function (args) {


        if (args.value === null) {
            document.getElementById("messageBank").style.display = "block";
        } else {
            document.getElementById("messageBank").style.display = "none";
            bankAccount = args.value
            bankAccountName = args.itemData.name
            bankAccountCode = args.itemData.code
        }
    },
    onSendRemittanceAdvice: function (args, args1) {

        var dialogProgress = document.getElementById("prompProgressDialog").ej2_instances[0]
        var dialog = document.getElementById("promptInfoDialog").ej2_instances[0]
        var grid = document.getElementById("Grid").ej2_instances[0]

        let payload = {
            selectedInvoices: args,
            payments: args1.invoices
        }

        if (CheckIfConnectedToXero()) {
            $.ajax({
                url: '/Xero/SendRemittanceAdvice',
                type: 'POST',
                data: JSON.stringify(payload),
                beforeSend: function () {
                    //dialog.hide()
                    //grid.showSpinner()
                },
                headers: {
                    'Content-Type': 'application/json; charset=utf-8'
                },
                success: (result) => {
                    if (result.isError) {
                        dialog.hide()
                        grid.hideSpinner()
                        operations.errorHandling(result)
                        return

                    } else {

                        //document.getElementById("promptContent").innerHTML = ""
                        //var dlgHeader = document.getElementById("promptInfoDialog_dialog-header")
                        //dlgHeader.classList.add("prompt-dlg-header-template")
                        //var dlgContent = document.getElementById("promptInfoDialog_dialog-content")
                        //dlgContent.classList.add("dlg-content-template")
                        //dialogProgress.hide()
                        //dialog.show()

                        //document.getElementById("progressStatus4").style.display = "block";
                        //document.getElementById("Prog4").style.display = "block";
                        document.getElementById("promptContent").innerHTML = ""
                        document.getElementById("progressStatus4").innerHTML = "<span style='color:green;'><b>Status:</b> Successfully Sent...</span>";

                        var count = 80;
                        var myInterval = setInterval(function () {
                            if (count <= 100) {
                                document.getElementById('Prog4').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';
                            } else {
                                dialog.hide()
                                clearInterval(myInterval)
                                document.getElementById("Grid").ej2_instances[0].toolbarModule.enableItems(['SendRemittances'], false)
                                return;
                            }
                            count += 1;
                        }, 100);

                    }
                },
                error: (error) => {
                    console.log(error);

                }
            })
        } else {
            tenantNotConnected()
        }

    },
    ExportBatchPaymentReportConfirmationForNewTransaction: function (dialogProgress, dialog, result) {

        var dialog = document.getElementById("promptInfoDialog").ej2_instances[0]
        document.getElementById("promptContent").innerHTML = "Do you want to a new transaction?"

        var dlgHeader = document.getElementById("promptInfoDialog_dialog-header")
        dlgHeader.classList.add("prompt-dlg-header-template")

        var dlgContent = document.getElementById("promptInfoDialog_dialog-content")
        dlgContent.classList.add("dlg-content-template")

        document.getElementById("progressStatus4").style.display = "none";
        document.getElementById("Prog4").style.display = "none";
        document.getElementById("btnPromptOkay").style.display = "initial";
        document.getElementById("btnPromptCancel").style.display = "initial";

        dialogProgress.hide()
        dialog.hide()
        dialog.show()

        document.getElementById("btnPromptOkay").onclick = function (e) {
            operations.onGetBills()
            dialogProgress.hide()
            dialog.hide()
            location.href = `/Xero/DownloadPDF/${result.fileName}`
            switch (countryCode) {
                case "AU":
                    document.getElementById("Grid").ej2_instances[0].toolbarModule.enableItems(['Export'], true)
                    break;
                case "NZ":
                    document.getElementById("Grid").ej2_instances[0].toolbarModule.enableItems(['ExportCSV'], true)
                    break;
                case "GB":
                    document.getElementById("Grid").ej2_instances[0].toolbarModule.enableItems(['ExportToBacs'], true)
                    break;
                default:
            }
            document.getElementById("Grid").ej2_instances[0].toolbarModule.enableItems(['MarkAsPaid', 'SendRemittances', 'ExportBatchPaymentReport'], false)
            disabledGridEditing = false
        }

        document.getElementById("btnPromptCancel").onclick = function (e) {
            location.href = `/Xero/DownloadPDF/${result.fileName}`
            dialogProgress.hide()
            dialog.hide()
        }

    },
    UpdatePaymentHistory: function (payments, dialogProgress) {

        let payload = {
            payments: payments.invoices
        }

        if (CheckIfConnectedToXero()) {
            $.ajax({
                url: '/Xero/UpdatePaymentHistory',
                type: 'POST',
                data: JSON.stringify(payments.invoices),
                headers: {
                    'Content-Type': 'application/json; charset=utf-8',
                    'Accept': 'application/json; charset=utf-8'
                },
                success: (result) => {
                    if (result.isError) {
                        dialogProgress.hide()
                        operations.errorHandling(result)
                        return

                    } else {

                        document.getElementById("progressStatusOnly").innerHTML = "<span style='color:green;'><b>Status:</b>  Successfully Transacted...</span>";
                        var count = 80;
                        let myInterval = setInterval(function () {

                            if (count <= 100) {
                                document.getElementById('ProgOnly').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';
                            } else {
                                dialogProgress.hide()
                                clearInterval(myInterval)
                                return;
                            }
                            count += 1;
                        }, 100);
                    }
                },
                error: (error) => {
                    let errorDetail = {
                        invoiceNumber: "",
                        errors: []
                    }
                    let errors = {
                        errorDetail: []
                    }

                    let payload = operations.extractJson(error.responseText)
                    payload.Elements.forEach((item) => {
                        errors.errorDetail.push({
                            invoiceNumber: item.InvoiceNumber,
                            errors: item.ValidationErrors
                        })
                    })

                    //operations.errorFormat(errors)
                    //grid.hideSpinner()
                }
            })
        } else {
            tenantNotConnected()
        }
    },
    errorHandling: function (result) {
        if (result.errorCode == 1) {
            if (result.sessionTimeOut) {
                window.location = '/Home/Index'
            }

            let payload = {
                ErrorCode: result.errorCode,
                ExceptionPath: result.exceptionPath,
                ExceptionMessage: result.exceptionMessage,
                Stacktrace: result.stacktrace
            }

            $.ajax({        
                url: '/ReportBug/ErrorDisplayJson/',
                type: 'POST',
                contentType: 'application/json',
                accept: 'application/json',
                data: JSON.stringify(payload),
                success: (result) => {
                    $("#BodyContent").html(result)
                    console.log(result)
                },
                error: (error) => {
                    console.log(error)
                }
            })
      
        }
        else {
            if (result.errorCode == 400) {
                let errorDetail = {
                    invoiceNumber: "",
                    errors: []
                }
                let errors = {
                    errorDetail: []

                }

                let payload = operations.extractJson(result.innerMessage)
                payload.Elements.forEach((item) => {
                    errors.errorDetail.push({
                        invoiceNumber: item.InvoiceNumber,
                        errors: item.ValidationErrors
                    })
                })
                //dialogProgress.hide()
                operations.errorFormat(errors)
                document.getElementById("Grid").ej2_instances[0].hideSpinner()
            }

            if (result.errorCode == 401 || result.errorCode == 503 || result.errorCode == 429) {

                 let payload = {
                     ErrorCode: result.errorCode,
                     ExceptionPath: "Xero Response Error",
                     ExceptionMessage: result.innerMessage,
                     Stacktrace: result.message
                 }

                 $.ajax({
                     url: '/ReportBug/ErrorDisplayJsonReauthorized/',
                     type: 'POST',
                     contentType: 'application/json',
                     accept: 'application/json',
                     data: JSON.stringify(payload),
                     success: (result) => {
                         $("#BodyContent").html(result)
                         console.log(result)
                     },
                     error: (error) => {
                         console.log(error)
                     }
                 })
            }

            if (result.errorCode == 429) {

                let payload = {
                    ErrorCode: result.errorCode,
                    ExceptionPath: "Xero Response Error",
                    ExceptionMessage: result.innerMessage,
                    Stacktrace: result.message
                }

                $.ajax({
                    url: '/ReportBug/ErrorDisplayJsonReauthorized/',
                    type: 'POST',
                    contentType: 'application/json',
                    accept: 'application/json',
                    data: JSON.stringify(payload),
                    success: (result) => {
                        $("#BodyContent").html(result)
                        console.log(result)
                    },
                    error: (error) => {
                        console.log(error)
                    }
                })
            }
        }
    },
    ExportBatchPaymentReport: function (args, args1) {

        var dialog = document.getElementById("promptInfoDialog").ej2_instances[0]
        var grid = document.getElementById("Grid").ej2_instances[0]

        var dialogProgress = document.getElementById("prompProgressDialog").ej2_instances[0]

        var dlgHeader = document.getElementById("prompProgressDialog_dialog-header")
        var dlgContent = document.getElementById("prompProgressDialog_dialog-content")

        dlgHeader.classList.add("prompt-dlg-header-template")
        dlgContent.classList.add("dlg-content-template")

        dialogProgress.show()

        document.getElementById("progressStatusOnly").innerHTML = "<span style='color:green;'><b>Status:</b>  Exporting Batch Payment report..</span>";

        var count = 0;
        var myInterval = setInterval(function () {

            if (count <= 50) {
                document.getElementById('ProgOnly').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';
            } else {
                clearInterval(myInterval)
                return;
            }
            count += 1;

        }, 100);

        let payload = {
            selectedInvoices: args,
            payments: args1.invoices
        }

        $.ajax({
            url: '/Xero/ProcessBatchReport',
            type: 'POST',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(payload),
            beforeSend: function () {
                //dialog.hide()
                //grid.showSpinner()
            },
            headers: {
                'Content-Type': 'application/json; charset=utf-8'
            },
            success: (result) => {
                if (result.isError) {
                    dialog.hide()
                    grid.hideSpinner()
                    operations.errorHandling(result)
                    return

                } else {

                    document.getElementById("progressStatusOnly").innerHTML = "<span style='color:green;'><b>Status:</b>  Successfully exported..</span>";

                    var count = 50;
                    var myInterval = setInterval(function () {

                        if (count <= 100) {
                            document.getElementById('ProgOnly').innerHTML = '<div class="progress-bar progress-bar-striped bg-success progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: ' + count + '%">' + count + '%</div>';
                        } else {

                            clearInterval(myInterval)
                            operations.ExportBatchPaymentReportConfirmationForNewTransaction(dialogProgress, dialog, result)
                            return;
                        }
                        count += 1;

                    }, 100);
                    //grid.hideSpinner()
                }
            },
            error: (error) => console.log(error)
        })
    }
}