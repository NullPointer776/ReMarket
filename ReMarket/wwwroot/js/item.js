var dataTable;

function escapeHtml(value) {
    if (value == null) return '';
    return String(value)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

$(document).ready(function () {
    var params = new URLSearchParams(window.location.search);
    loadDataTable(params.get('status') || '');
});

function loadDataTable(status) {
    var url = '/Admin/Item/GetAll';
    if (status) {
        url += '?status=' + encodeURIComponent(status);
    }

    if ($.fn.DataTable.isDataTable('#tblData')) {
        $('#tblData').DataTable().destroy();
    }

    dataTable = $('#tblData').DataTable({
        ajax: { url: url, dataSrc: 'data' },
        columns: [
            {
                data: 'id',
                render: function (data) {
                    return `<span class="fw-bold text-primary">${escapeHtml(data)}</span>`;
                },
                width: '5%'
            },
            {
                data: 'name',
                render: function (data, type, row) {
                    return `<div class="fw-semibold">${escapeHtml(data)}</div><div class="small text-muted"><code>${escapeHtml(row.slug)}</code></div>`;
                },
                width: '20%'
            },
            {
                data: 'seller.email',
                defaultContent: '',
                render: function (data) {
                    return escapeHtml(data);
                },
                width: '15%'
            },
            {
                data: 'category.name',
                defaultContent: '-',
                render: function (data) {
                    return escapeHtml(data || '-');
                },
                width: '10%'
            },
            {
                data: 'price',
                render: function (data) {
                    return new Intl.NumberFormat('en-NZ', { style: 'currency', currency: 'NZD' }).format(data);
                },
                width: '8%'
            },
            { data: 'quantity', width: '5%' },
            {
                data: 'status',
                render: function (data) {
                    switch (data) {
                        case 'Pending': return '<span class="badge bg-warning text-dark">Pending</span>';
                        case 'Available': return '<span class="badge bg-success">Available</span>';
                        case 'Rejected': return '<span class="badge bg-danger">Rejected</span>';
                        case 'SoldOut': return '<span class="badge bg-info">Sold Out</span>';
                        default: return escapeHtml(data);
                    }
                },
                width: '8%'
            },
            {
                data: 'id',
                orderable: false,
                render: function (data, type, row) {
                    var id = escapeHtml(data);
                    var html = `<div class="btn-group flex-wrap" role="group">
                        <a href="/Admin/Item/Details/${id}" class="btn btn-sm btn-outline-secondary mx-1">Details</a>
                        <a href="/Admin/Item/Edit/${id}" class="btn btn-sm btn-outline-primary mx-1"><i class="bi bi-pencil-square"></i> Edit</a>`;

                    if (row.status === 'Pending') {
                        html += `<button type="button" class="btn btn-sm btn-success mx-1" onclick="approveItem(${id})">Approve</button>
                        <a href="/Admin/Item/Reject/${id}" class="btn btn-sm btn-outline-warning mx-1">Reject</a>`;
                    }

                    html += `<button type="button" class="btn btn-sm btn-outline-danger mx-1" onclick="deleteItem('/Admin/Item/Delete/${id}')"><i class="bi bi-trash-fill"></i> Delete</button>
                    </div>`;
                    return html;
                },
                width: '22%'
            }
        ]
    });
}

function approveItem(id) {
    var token = $('input[name="__RequestVerificationToken"]').val();
    $.post('/Admin/Item/Approve', { id: id, __RequestVerificationToken: token })
        .done(function () {
            toastr.success('Item approved.');
            dataTable.ajax.reload();
        })
        .fail(function () {
            toastr.error('Could not approve item.');
        });
}

function deleteItem(url) {
    Swal.fire({
        title: 'Delete this item?',
        text: 'Images on disk will also be removed.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Yes, delete it'
    }).then(function (result) {
        if (!result.isConfirmed) return;

        var token = $('input[name="__RequestVerificationToken"]').val();
        $.ajax({
            url: url,
            type: 'DELETE',
            headers: { 'RequestVerificationToken': token },
            success: function (data) {
                if (data.success) {
                    toastr.success(data.message);
                    dataTable.ajax.reload();
                } else {
                    toastr.error(data.message);
                }
            },
            error: function () {
                toastr.error('Delete request failed.');
            }
        });
    });
}
