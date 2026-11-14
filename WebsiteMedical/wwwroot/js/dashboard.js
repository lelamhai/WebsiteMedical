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

// Tự động load họ và tên vào class .user-name
document.addEventListener('DOMContentLoaded', function () {
    function getCookie(name) {
        const value = `; ${document.cookie}`;
        const parts = value.split(`; ${name}=`);
        if (parts.length === 2) {
            try {
                return decodeURIComponent(parts.pop().split(';').shift());
            } catch (e) {
                return null;
            }
        }
        return null;
    }

    const savedName = getCookie('UserName') || localStorage.getItem('user_name');
    if (savedName && savedName.trim() !== '') {
        const cleanName = savedName.trim();
        document.querySelectorAll('.user-name').forEach(function (el) {
            el.textContent = cleanName;
        });

        // Cập nhật chữ cái đầu cho avatar nếu chưa có
        const avatar = document.querySelector('.user-avatar');
        if (avatar && avatar.textContent.trim() === 'A' || avatar && avatar.textContent.trim() === 'BS') {
            avatar.textContent = cleanName.charAt(0).toUpperCase();
        }
    }
});

