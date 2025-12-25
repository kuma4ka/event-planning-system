export function initDashboard() {
    window.copyLink = function(url) {
        if (!navigator.clipboard) {
            console.warn('Clipboard API not available');
            return;
        }
        navigator.clipboard.writeText(url).then(() => {
            alert('Link copied to clipboard!');
        }, (err) => {
            console.error('Could not copy text: ', err);
        });
    };

    const deleteModal = document.getElementById('deleteModal');

    if (deleteModal) {
        deleteModal.addEventListener('show.bs.modal', (event) => {
            const button = event.relatedTarget;

            const eventId = button.getAttribute('data-event-id');
            const eventName = button.getAttribute('data-event-name');

            const modalTitle = deleteModal.querySelector('#modalEventName');
            const deleteForm = deleteModal.querySelector('#deleteForm');

            modalTitle.textContent = eventName;

            const urlTemplate = deleteModal.getAttribute('data-url-template');

            if (urlTemplate) {
                deleteForm.action = urlTemplate.replace('PLACEHOLDER', eventId);
            }
        });
    }
}