document.addEventListener("DOMContentLoaded", function () {

    InitializeGrid();

})

//initialize grid 
function InitializeGrid() {

    SearchXeroApiLogs()
    //let grid = document.getElementById("listApiLogs").ej2_instances[0]

    //var Tenant = document.getElementById("Tenant").ej2_instances[0];

    //Tenant.value = Tenant.value == null ? "" : Tenant.value;

    //let tenantSearch = {
    //    TenantName: Tenant.value,
    //}

    //grid.showSpinner()

    //var ajax = new ej.base.Ajax({
    //    url: '/XeroApiLogs/SearchXeroApiLogs',
    //    type: 'POST',
    //    data: JSON.stringify(tenantSearch),
    //    contentType: 'application/json',
    //    accept: 'application/json'
    //})
    //ajax.send()
    //ajax.onSuccess = function (data) {
    //    grid.dataSource = JSON.parse(data)
    //    grid.hideSpinner();
    //}

}

//btn search events
document.getElementById("btnSearch").addEventListener('click', function () {

    SearchXeroApiLogs()
});


function SearchXeroApiLogs() {


    let grid = document.getElementById("listApiLogs").ej2_instances[0]

    var Tenant = document.getElementById("Tenant").ej2_instances[0];

    Tenant.value = Tenant.value == null ? "" : Tenant.value;

    let tenantSearch = {
        TenantName: Tenant.value,
    }

    grid.showSpinner()

    var ajax = new ej.base.Ajax({
        url: '/XeroApiLogs/SearchXeroApiLogs',
        type: 'POST',
        data: JSON.stringify(tenantSearch),
        contentType: 'application/json',
        accept: 'application/json'
    })
    ajax.send()
    ajax.onSuccess = function (data) {
        grid.dataSource = JSON.parse(data)
        grid.hideSpinner();
    }


}


