export function initEventActions() {
    initDeleteModal();
    initCopyLinks();
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

function initCopyLinks() {
    document.addEventListener('click', function (e) {
        const target = e.target.closest('.js-copy-link');
        if (!target) return;

        e.preventDefault();
        const url = target.getAttribute('data-url');

        if (url) {
            navigator.clipboard.writeText(url).then(() => {
                showToast('Link copied to clipboard!');
            }).catch(err => {
                console.error('Failed to copy: ', err);
            });
        }
    });
}

function showToast(message) {
    alert(message);
}