(function () {
    'use strict';

    var dataTable;
    var STATUS_FILTERS = ['inprocess', 'completed', 'pending', 'approved'];

    function escapeHtml(value) {
        if (value == null) return '';
        return String(value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function detectStatus() {
        var query = window.location.search;
        for (var i = 0; i < STATUS_FILTERS.length; i++) {
            if (query.includes(STATUS_FILTERS[i])) {
                return STATUS_FILTERS[i];
            }
        }
        return 'all';
    }

    function loadDataTable(status) {
        if ($.fn.DataTable.isDataTable('#tblData')) {
            $('#tblData').DataTable().destroy();
        }

        dataTable = $('#tblData').DataTable({
            ajax: { url: '/Admin/Order/GetAll?status=' + status },
            columns: [
                { data: 'id', width: '5%' },
                { data: 'name', width: '20%', render: escapeHtml },
                { data: 'phoneNumber', width: '15%', render: escapeHtml },
                { data: 'applicationUser.email', width: '20%', render: escapeHtml },
                { data: 'orderStatus', width: '10%', render: escapeHtml },
                { data: 'paymentStatus', width: '10%', render: escapeHtml },
                {
                    data: 'orderTotal',
                    width: '10%',
                    render: function (data) {
                        return new Intl.NumberFormat('en-NZ', { style: 'currency', currency: 'NZD' }).format(data);
                    }
                },
                {
                    data: 'id',
                    render: function (data) {
                        return '<div class="btn-group" role="group">' +
                            '<a href="/Admin/Order/Details?orderId=' + escapeHtml(data) + '" class="btn btn-primary btn-sm mx-1">' +
                            '<i class="bi bi-pencil-square"></i> Details</a></div>';
                    },
                    width: '10%',
                    orderable: false
                }
            ]
        });
    }

    $(document).ready(function () {
        loadDataTable(detectStatus());
    });
})();
