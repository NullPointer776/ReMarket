var dataTable;

$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("inprocess")) {
        loadDataTable("inprocess");
    } else if (url.includes("completed")) {
        loadDataTable("completed");
    } else if (url.includes("pending")) {
        loadDataTable("pending");
    } else if (url.includes("approved")) {
        loadDataTable("approved");
    } else {
        loadDataTable("all");
    }
});

function loadDataTable(status) {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/Admin/Order/GetAll?status=' + status },
        "columns": [
            { data: 'id', width: "5%" },
            { data: 'name', width: "20%" },
            { data: 'phoneNumber', width: "15%" },
            { data: 'applicationUser.email', width: "20%" },
            { data: 'orderStatus', width: "10%" },
            { data: 'paymentStatus', width: "10%" },
            {
                data: 'orderTotal',
                width: "10%",
                render: function (data) {
                    return new Intl.NumberFormat('en-NZ', { style: 'currency', currency: 'NZD' }).format(data);
                }
            },
            {
                data: 'id',
                render: function (data) {
                    return `<div class="btn-group" role="group">
                        <a href="/Admin/Order/Details?orderId=${data}" class="btn btn-primary btn-sm mx-1">
                            <i class="bi bi-pencil-square"></i> Details
                        </a>
                    </div>`;
                },
                width: "10%",
                orderable: false
            }
        ]
    });
}
