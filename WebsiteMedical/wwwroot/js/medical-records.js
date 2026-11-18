let screeningData = []; // Lưu dữ liệu toàn bộ để phục vụ search

document.addEventListener('DOMContentLoaded', function() {
    const campaignSelect = document.getElementById('campaignSelect');
    const searchInput = document.getElementById('searchInput');
    const searchBtn = document.getElementById('searchBtn');

    campaignSelect.addEventListener('change', function() {
        const campaignId = this.value;
        console.log('Campaign selected:', campaignId);
        searchInput.value = ''; // Clear search khi chọn chiến dịch mới

        if (!campaignId) {
            // Reset table khi không chọn
            document.getElementById('screeningTableBody').innerHTML = `
                <tr>
                    <td colspan="6" class="text-center text-muted py-4">
                        Vui lòng chọn một chiến dịch để xem danh sách kết quả sàng lọc
                    </td>
                </tr>
            `;
            screeningData = [];
            return;
        }

        loadHealthScreenings(campaignId);
    });

    // Search functionality
    searchBtn.addEventListener('click', function() {
        const searchTerm = searchInput.value.trim().toLowerCase();
        filterScreenings(searchTerm);
    });

    // Search on Enter key
    searchInput.addEventListener('keypress', function(e) {
        if (e.key === 'Enter') {
            const searchTerm = searchInput.value.trim().toLowerCase();
            filterScreenings(searchTerm);
        }
    });

    document.getElementById('examModal').addEventListener('show.bs.modal', function(event) {
        const updateButton = event.relatedTarget;
        const userId = updateButton?.dataset.userId;
        const screeningId = updateButton?.dataset.screeningId;
        document.getElementById('screeningId').value = screeningId || '';

        const screening = screeningData.find(item =>
            String(item.healthScreeningId ?? item.screeningId ?? item.id) === String(screeningId));
        const isFailed = screening?.screeningResult === 'Failed';
        document.getElementById('resultPass').checked = !isFailed;
        document.getElementById('resultFail').checked = isFailed;
        document.querySelectorAll('#failReasonSection input[type="checkbox"]').forEach(checkbox => {
            checkbox.checked = false;
        });
        document.getElementById('rejectionReasonNote').value = screening?.rejectionReason || '';
        updateExamSections();

        if (userId) {
            loadVolunteerInfo(userId);
        }
    });

    document.getElementById('saveExamResultBtn').addEventListener('click', saveHealthScreeningResult);
    document.querySelectorAll('input[name="examResult"]').forEach(radio => {
        radio.addEventListener('change', updateExamSections);
    });
    updateExamSections();
});

function loadHealthScreenings(campaignId) {
    campaignId = 1;
    const tableBody = document.getElementById('screeningTableBody');
    tableBody.innerHTML = '<tr><td colspan="6" class="text-center text-muted py-4">Đang tải...</td></tr>';

    // Gọi endpoint proxy của backend thay vì gọi trực tiếp API (tránh CORS)
    const proxyUrl = `/api/staff/health-screenings?campaignId=${campaignId}`;
    console.log('Fetching from proxy:', proxyUrl);

    fetch(proxyUrl, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        },
        credentials: 'include' // Gửi cookies
    })
    .then(response => {
        console.log('Response status:', response.status);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return response.json();
    })
    .then(data => {
        console.log('API Response:', data);
        if (Array.isArray(data.data) && data.data.length > 0) {
            screeningData = data.data; // Lưu dữ liệu gốc
            renderScreenings(screeningData);
        } else {
            tableBody.innerHTML = `
                <tr>
                    <td colspan="6" class="text-center text-muted py-4">
                        Không có kết quả sàng lọc cho chiến dịch này
                    </td>
                </tr>
            `;
            screeningData = [];
        }
    })
    .catch(error => {
        console.error('Error loading screenings:', error);
        tableBody.innerHTML = `
            <tr>
                <td colspan="6" class="text-center text-danger py-4">
                    Lỗi tải dữ liệu: ${error.message}<br>
                    <small>Hãy mở Developer Tools (F12) để xem chi tiết</small>
                </td>
            </tr>
        `;
        screeningData = [];
    });
}

function renderScreenings(data) {
    const tableBody = document.getElementById('screeningTableBody');

    if (!data || data.length === 0) {
        tableBody.innerHTML = `
            <tr>
                <td colspan="6" class="text-center text-muted py-4">
                    Không tìm thấy kết quả khớp với tìm kiếm
                </td>
            </tr>
        `;
        return;
    }

    let html = '';
    data.forEach(screening => {
            const screeningId = screening.healthScreeningId ?? screening.screeningId ?? screening.id;
            const statusConfig = {
                Passed: {
                    rowClass: '',
                    badge: '<span class="badge-status badge-active">✓ ĐẠT ĐIỀU KIỆN</span>'
                },
                Failed: {
                    rowClass: 'table-danger-subtle',
                    badge: '<span class="badge-status badge-fail">✕ KHÔNG ĐẠT</span>'
                },
                None: {
                    rowClass: 'table-warning-subtle', // Hoặc để '' nếu không muốn tô màu nền hàng
                    badge: '<span class="badge-status badge-pending">⏳ CHỜ KHÁM</span>'
                }
            };

            // Lấy cấu hình theo kết quả, mặc định là 'None' nếu dữ liệu bị null/undefined
            const currentStatus = statusConfig[screening.screeningResult] || statusConfig.None;

            const rowClass = currentStatus.rowClass;
            const statusBadge = currentStatus.badge;
            const bloodVolumeDisplay = screening.bloodVolume || '-';

        html += `
            <tr class="${rowClass}" style="background-color: ${rowClass ? '#fffafa' : ''};">
                <td class="fw-semibold">${screening.volunteerId}</td>
                <td>${screening.volunteerName}</td>
                <td>${statusBadge}</td>
                <td class="text-center">${bloodVolumeDisplay}</td>
                <td>
                    ${screening.rejectionReason ? `<span class="text-danger small">${screening.rejectionReason}</span>` : '<span class="text-muted">-</span>'}
                </td>
                <td class="text-end">
                    <button class="btn btn-outline-secondary btn-sm" data-user-id="${screening.volunteerId}" data-screening-id="${screeningId ?? ''}" data-bs-toggle="modal" data-bs-target="#examModal">
                        Cập nhật
                    </button>
                </td>
            </tr>
        `;
    });
    tableBody.innerHTML = html;
}

function updateExamSections() {
    const isPassed = document.getElementById('resultPass').checked;
    document.getElementById('failReasonSection').classList.toggle('d-none', isPassed);
    document.getElementById('passVolumeSection').classList.toggle('d-none', !isPassed);
}

async function saveHealthScreeningResult() {
    const screeningId = document.getElementById('screeningId').value;
    const isPassed = document.getElementById('resultPass').checked;
    const screeningResult = isPassed ? 'Passed' : 'Failed';
    const reasons = Array.from(document.querySelectorAll('#failReasonSection input[type="checkbox"]:checked'))
        .map(checkbox => checkbox.value.trim())
        .filter(Boolean);
    const note = document.getElementById('rejectionReasonNote').value.trim();
    const rejectionReason = isPassed ? '' : [...reasons, ...(note ? [note] : [])].join('-');

    if (!/^\d+$/.test(screeningId)) {
        alert('Không tìm thấy mã hồ sơ sàng lọc để cập nhật.');
        return;
    }

    if (!isPassed && !rejectionReason) {
        alert('Vui lòng chọn hoặc nhập lý do không đạt.');
        return;
    }

    const saveButton = document.getElementById('saveExamResultBtn');
    saveButton.disabled = true;

    try {
        const response = await fetch(`/api/staff/health-screenings/${encodeURIComponent(screeningId)}`, {
            method: 'PUT',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            credentials: 'same-origin',
            body: JSON.stringify({ screeningResult, rejectionReason })
        });
        const responseText = await response.text();
        let result = {};
        try {
            result = responseText ? JSON.parse(responseText) : {};
        } catch {
            result = { message: responseText };
        }

        if (!response.ok) {
            throw new Error(result.message || result.Message || `HTTP error! status: ${response.status}`);
        }

        const record = screeningData.find(item =>
            String(item.healthScreeningId ?? item.screeningId ?? item.id) === String(screeningId));
        if (record) {
            record.screeningResult = screeningResult;
            record.rejectionReason = rejectionReason;
        }
        renderScreenings(screeningData);

        const modalElement = document.getElementById('examModal');
        bootstrap.Modal.getOrCreateInstance(modalElement).hide();
        alert(result.message || result.Message || 'Đã cập nhật kết quả khám thành công.');
    } catch (error) {
        console.error('Error updating health screening:', error);
        alert(`Không thể lưu kết quả khám: ${error.message}`);
    } finally {
        saveButton.disabled = false;
    }
}

function filterScreenings(searchTerm) {
    if (!searchTerm) {
        // Nếu search rỗng, hiển thị tất cả
        renderScreenings(screeningData);
        return;
    }

    // Lọc dữ liệu theo tên hoặc mã TNV
    const filtered = screeningData.filter(screening => {
        const name = screening.volunteerName.toLowerCase();
        const id = screening.volunteerId.toString().toLowerCase();
        return name.includes(searchTerm) || id.includes(searchTerm);
    });

    renderScreenings(filtered);
}

function loadVolunteerInfo(userId) {
    const volunteerInfo = document.getElementById('volunteerInfo');
    const volunteerDetails = document.getElementById('volunteerDetails');
    volunteerInfo.textContent = 'Đang tải thông tin tình nguyện viên...';
    volunteerDetails.textContent = '';

    // Use the MVC same-origin proxy so the browser does not need CORS permission
    // from the separate backend API. The bearer token is still forwarded by MVC.
    fetch(`/api/staff/users/${encodeURIComponent(userId)}`, {
        method: 'GET',
        headers: {
            'Accept': 'application/json'
        },
        // Same-origin requests send the AccessToken cookie used by other MVC proxies.
        credentials: 'same-origin'
    })
    .then(response => {
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        return response.json();
    })
    .then(result => {
        const user = result.data;
        const gender = user.gender === 'Male' ? 'Nam' : user.gender === 'Female' ? 'Nữ' : user.gender || '-';
        const birthYear = user.dateOfBirth ? new Date(user.dateOfBirth).getFullYear() : '-';

        volunteerInfo.textContent = `Tình nguyện viên: ${user.fullName || `${user.firstName || ''} ${user.lastName || ''}`.trim()} (TNV-${user.userId})`;
        volunteerDetails.textContent = `Email: ${user.email || '-'} • SĐT: ${user.phone || '-'} • ${gender} • Sinh năm ${birthYear}`;
    })
    .catch(error => {
        console.error('Error loading volunteer information:', error);
        volunteerInfo.textContent = 'Không thể tải thông tin tình nguyện viên';
        volunteerDetails.textContent = error.message;
    });
}
