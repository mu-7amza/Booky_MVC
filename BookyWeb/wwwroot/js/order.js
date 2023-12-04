var dataTable;
$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("inprocess")) {
        loadDataTable("inprocess");
    }
    else {
        if (url.includes("completed")) {
            loadDataTable("completed");
        }
        else {
            if (url.includes("pending")) {
                loadDataTable("pending");
            }
            else {
                if (url.includes("approved")) {
                    loadDataTable("approved");
                }
                else {
                    loadDataTable("all");
                }
            }
        }
    }
});

function loadDataTable(status) {
    dataTable = $('#myTable').DataTable({
        "ajax": { url: '/admin/order/getall?status='+ status },
        "columns":
        [
            { data: 'id' , "width":"5%" },
            { data: 'fullName', "width": "20%" },
            { data: 'phoneNumber', "width": "25%" },
            { data: 'applicationUser.email', "width": "25%" },
            { data: 'orderStatus', "width": "10%" },
            { data: 'totalOrder', "width": "10%" },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75" btn-group role="group">
                            <a class="btn btn-primary mx-2" href="/admin/order/details?orderId=${data}"><i class="bi bi-pencil-square"></i></a>
                            `
                },
                "width":"5%"
            }

        ]
    });
}

