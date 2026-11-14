/**
 * Login Module - JavaScript xử lý giao diện đăng nhập & kết quả
 */

// Bật / tắt ẩn hiện mật khẩu
function togglePasswordVisibility() {
    const pwdInput = document.getElementById('password');
    const eyeIcon = document.getElementById('eyeIcon');
    if (!pwdInput || !eyeIcon) return;

    if (pwdInput.type === 'password') {
        pwdInput.type = 'text';
        eyeIcon.innerHTML = `
            <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"></path>
            <line x1="1" y1="1" x2="23" y2="23"></line>
        `;
    } else {
        pwdInput.type = 'password';
        eyeIcon.innerHTML = `
            <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path>
            <circle cx="12" cy="12" r="3"></circle>
        `;
    }
}

// Hiệu ứng chờ khi form submit về Controller
function onFormSubmit() {
    const btn = document.getElementById('submitBtn');
    const spinner = document.getElementById('btnSpinner');
    const btnText = document.getElementById('btnText');
    const btnArrow = document.getElementById('btnArrowIcon');

    if (btn && spinner && btnText && btnArrow) {
        btn.disabled = true;
        spinner.classList.remove('d-none');
        btnArrow.classList.add('d-none');
        btnText.textContent = "Đang xử lý...";
    }
}

// Sao chép toàn bộ JSON
function copyJsonToClipboard() {
    const pre = document.getElementById('jsonViewer');
    if (!pre) return;
    navigator.clipboard.writeText(pre.textContent).then(() => {
        showToast("Đã sao chép toàn bộ JSON!");
    }).catch(() => {
        showToast("Không thể sao chép!");
    });
}

// Sao chép riêng Access Token
function copyTokenOnly() {
    const pre = document.getElementById('jsonViewer');
    const token = pre ? pre.getAttribute('data-access-token') : null;
    const finalToken = token || localStorage.getItem('accessToken');

    if (finalToken) {
        navigator.clipboard.writeText(finalToken).then(() => {
            showToast("Đã sao chép Access Token!");
        }).catch(() => {
            showToast("Không thể sao chép token!");
        });
    } else {
        showToast("Không tìm thấy Access Token!");
    }
}

// Tải file JSON về máy (như nút Download trong ảnh)
function downloadJsonResponse() {
    const pre = document.getElementById('jsonViewer');
    if (!pre) return;
    const blob = new Blob([pre.textContent], { type: "application/json" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = "login-response.json";
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
    showToast("Đã tải file login-response.json");
}

// Hiển thị thông báo Toast
function showToast(text) {
    const toast = document.getElementById('toastCopy');
    const toastText = document.getElementById('toastCopyText');
    if (!toast || !toastText) return;

    toastText.textContent = text;
    toast.style.display = 'flex';
    setTimeout(() => {
        toast.style.display = 'none';
    }, 2500);
}

// Tự động chạy khi trang tải xong
document.addEventListener("DOMContentLoaded", function () {
    const preElement = document.getElementById('jsonViewer');
    if (!preElement) return;

    // 1. Lưu token & thông tin người dùng vào localStorage nếu đăng nhập thành công
    const accessToken = preElement.getAttribute('data-access-token');
    const refreshToken = preElement.getAttribute('data-refresh-token');
    const refreshExpiresAt = preElement.getAttribute('data-expires');
    const userName = preElement.getAttribute('data-user-name');
    const userRole = preElement.getAttribute('data-user-role');

    if (accessToken) {
        try {
            localStorage.setItem('accessToken', accessToken);
            if (refreshToken) localStorage.setItem('refreshToken', refreshToken);
            if (refreshExpiresAt) localStorage.setItem('refreshExpiresAt', refreshExpiresAt);
            if (userName) localStorage.setItem('user_name', userName);
            if (userRole) localStorage.setItem('user_role', userRole);
        } catch (e) {
            console.warn("Không thể lưu localStorage:", e);
        }
    }

    // 2. Tô màu cú pháp JSON hiển thị giống màn hình kết quả trong hình
    const rawText = preElement.textContent;
    if (rawText && rawText.trim().length > 0) {
        const safeStr = rawText.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
        const highlighted = safeStr.replace(/("(\\u[a-zA-Z0-9]{4}|\\[^u]|[^\\"])*"(\s*:)?|\b(true|false|null)\b|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?)/g, function (match) {
            let cls = 'json-string';
            if (/^"/.test(match)) {
                if (/:$/.test(match)) {
                    cls = 'json-key';
                } else if (match.length > 50) {
                    cls = 'json-token';
                } else {
                    cls = 'json-string';
                }
            }
            return '<span class="' + cls + '">' + match + '</span>';
        });
        preElement.innerHTML = highlighted;
    }
});

