/**
 * Dashboard Scripts - Xử lý menu responsive và tương tác chung
 */

function toggleSidebar() {
    const sidebar = document.getElementById('dashSidebar');
    if (sidebar) {
        sidebar.classList.toggle('open');
    }
}

// Đóng sidebar khi click ngoài trên màn hình nhỏ
document.addEventListener('click', function (e) {
    const sidebar = document.getElementById('dashSidebar');
    const toggleBtn = document.getElementById('btnMenuToggle');
    if (!sidebar || !toggleBtn) return;

    if (window.innerWidth <= 992 && sidebar.classList.contains('open')) {
        if (!sidebar.contains(e.target) && !toggleBtn.contains(e.target)) {
            sidebar.classList.remove('open');
        }
    }
});

