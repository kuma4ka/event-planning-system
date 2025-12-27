export function initVenueManagement() {
    const deleteModal = document.getElementById('deleteModal');

    if (deleteModal) {
        deleteModal.addEventListener('show.bs.modal', (event) => {
            const button = event.relatedTarget;

            const venueId = button.getAttribute('data-venue-id');
            const venueName = button.getAttribute('data-venue-name');

            const modalTitle = deleteModal.querySelector('#modalVenueName');
            const deleteForm = deleteModal.querySelector('#deleteForm');

            if (modalTitle) {
                modalTitle.textContent = venueName;
            }

            const urlTemplate = deleteModal.getAttribute('data-url-template');

            if (urlTemplate && deleteForm) {
                deleteForm.action = urlTemplate.replace('PLACEHOLDER', venueId);
            }
        });
    }
}