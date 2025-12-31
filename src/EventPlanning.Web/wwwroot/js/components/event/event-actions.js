export function initEventActions() {
    initDeleteModal();
    initShareModal();
}

function initDeleteModal() {
    const deleteModalEl = document.getElementById('deleteModal');
    if (!deleteModalEl) return;

    deleteModalEl.addEventListener('show.bs.modal', function (event) {
        const button = event.relatedTarget;

        const eventId = button.getAttribute('data-event-id');
        const eventName = button.getAttribute('data-event-name');

        const modalTitle = deleteModalEl.querySelector('#modalEventName');
        const deleteForm = deleteModalEl.querySelector('#deleteForm');
        const urlTemplate = deleteModalEl.getAttribute('data-url-template');

        if (modalTitle) modalTitle.textContent = eventName;

        if (deleteForm && urlTemplate) {
            deleteForm.action = urlTemplate.replace('/0', '/' + eventId);
        }
    });
}

function initShareModal() {
    const shareModalEl = document.getElementById('shareModal');
    if (!shareModalEl) return;

    const shareInput = shareModalEl.querySelector('#shareInput');
    const copyBtn = shareModalEl.querySelector('#btnCopyShare');
    const successMsg = shareModalEl.querySelector('#copySuccessMsg');

    shareModalEl.addEventListener('show.bs.modal', function (event) {
        const button = event.relatedTarget;
        const url = button.getAttribute('data-event-url');

        if (shareInput) {
            shareInput.value = url;
            // Reset state
            successMsg.classList.remove('opacity-100');
            successMsg.classList.add('opacity-0');
        }
    });

    if (copyBtn && shareInput) {
        copyBtn.addEventListener('click', function () {
            shareInput.select();
            shareInput.setSelectionRange(0, 99999); // For mobile devices

            navigator.clipboard.writeText(shareInput.value).then(() => {
                successMsg.classList.remove('opacity-0');
                successMsg.classList.add('opacity-100');

                setTimeout(() => {
                    successMsg.classList.remove('opacity-100');
                    successMsg.classList.add('opacity-0');
                }, 2000);
            }).catch(err => {
                console.error('Failed to copy: ', err);
            });
        });
    }
}