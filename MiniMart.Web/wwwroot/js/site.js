function khoiTaoBangDuLieu(tenBangId) {
    if ($('#' + tenBangId).length) {
        var bang = $('#' + tenBangId).DataTable({
            language: {
                processing: "Đang xử lý...",
                search: "Tìm kiếm:",
                searchPlaceholder: "Nhập từ khóa...",
                lengthMenu: "Hiển thị _MENU_ dòng / trang",
                info: "Đang hiển thị _START_ đến _END_ trong tổng số _TOTAL_ dòng",
                infoEmpty: "Không có dữ liệu",
                infoFiltered: "(được lọc từ _MAX_ tổng số dòng)",
                loadingRecords: "Đang tải...",
                zeroRecords: "Không tìm thấy dữ liệu phù hợp",
                emptyTable: "Không có dữ liệu trong bảng",
                paginate: {
                    first: "Đầu",
                    previous: "Trước",
                    next: "Sau",
                    last: "Cuối"
                }
            },
            lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "Tất cả"]],
            pageLength: 10,
            order: [[1, 'asc']]
        });

        // Kích hoạt tính năng lọc văn bản cho các ô input
        bang.columns().every(function () {
            var that = this;
            $('input', this.header()).on('keyup change clear', function () {
                if (that.search() !== this.value) {
                    that.search(this.value).draw();
                }
            });
        });
    }
}