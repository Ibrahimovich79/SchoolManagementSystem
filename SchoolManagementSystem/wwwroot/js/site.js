// Generic Inline Editing Functions

window.enableEdit = function (fieldName) {
    var viewEl = document.getElementById('view-' + fieldName);
    var editEl = document.getElementById('edit-' + fieldName);
    if (viewEl) viewEl.style.display = 'none';
    if (editEl) editEl.style.display = 'block';
}

window.cancelEdit = function (fieldName) {
    var viewEl = document.getElementById('view-' + fieldName);
    var editEl = document.getElementById('edit-' + fieldName);
    if (editEl) editEl.style.display = 'none';
    if (viewEl) viewEl.style.display = 'block'; // Was 'flex' in some designs, but block/default is safer unless d-flex class used
    // Actually, the view container has d-flex class in HTML, so removing display:none reverts to d-flex via CSS if class is present? 
    // No, style="display: block" overrides class. 
    // Let's explicitly set it to '' (empty) to let CSS classes take over, or 'flex' if we know it's flex.
    // Given the HTML: <div id="view-X" class="d-flex ...">
    if (viewEl) viewEl.style.display = '';
}

window.saveField = async function (id, fieldName, handlerUrl) {
    const inputElement = document.querySelector(`#edit-${fieldName} input`);
    const newValue = inputElement.value;
    const antiforgeryToken = document.querySelector('input[name="__RequestVerificationToken"]').value;

    const formData = new FormData();
    formData.append('id', id);
    formData.append('fieldName', fieldName);
    formData.append('fieldValue', newValue);

    try {
        const response = await fetch(handlerUrl, {
            method: 'POST',
            headers: {
                'RequestVerificationToken': antiforgeryToken
            },
            body: formData
        });

        if (response.ok) {
            const result = await response.json();
            if (result.success) {
                if (result.shouldReload) {
                    if (result.redirectUrl) {
                        window.location.href = result.redirectUrl;
                    } else {
                        window.location.reload();
                    }
                    return;
                }

                // Update view text
                const viewElement = document.getElementById('view-' + fieldName);
                if (viewElement) {
                    // Structure: <span>Value</span> <i ...>
                    // We want to update the span.
                    const span = viewElement.querySelector('span');
                    if (span) span.innerText = result.newValue;
                    else {
                        // Fallback
                        viewElement.innerHTML = `<span class="me-2">${result.newValue}</span> <i class="fas fa-pencil-alt text-primary" style="cursor:pointer;" onclick="enableEdit('${fieldName}')"></i>`;
                    }
                }

                showToast('Success', 'Field updated successfully.');
                cancelEdit(fieldName);
            } else {
                alert('Update failed: ' + result.message);
            }
        } else {
            alert('Server error occurred.');
        }
    } catch (error) {
        console.error('Error:', error);
        alert('An error occurred while saving.');
    }
}

window.showToast = function (title, message) {
    let toastContainer = document.getElementById('toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toast-container';
        toastContainer.style.position = 'fixed';
        toastContainer.style.top = '20px';
        toastContainer.style.right = '20px';
        toastContainer.style.zIndex = '9999';
        document.body.appendChild(toastContainer);
    }

    const toast = document.createElement('div');
    toast.className = 'alert alert-success alert-dismissible fade show shadow';
    toast.innerHTML = `
        <strong>${title}</strong> ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;
    toastContainer.appendChild(toast);

    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 150);
    }, 3000);
}
