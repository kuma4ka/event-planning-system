/**
 * Initializes a generic delete modal that updates its form action and displayed name dynamically.
 * @param {string} modalId - The ID of the modal element.
 * @param {string} nameTargetId - The selector for the element displaying the item name (e.g. #modalEventName).
 * @param {string} formSelector - The selector for the form element.
 * @param {string} dataIdAttribute - The data attribute on the button containing the ID (e.g. data-event-id).
 * @param {string} dataNameAttribute - The data attribute on the button containing the Name (e.g. data-event-name).
 */
export function initDeleteModal(modalId, nameTargetId, formSelector, dataIdAttribute, dataNameAttribute) {
    const deleteModal = document.getElementById(modalId);

    if (deleteModal) {
        deleteModal.addEventListener('show.bs.modal', (event) => {
            const button = event.relatedTarget;
            if (!button) return;

            const id = button.getAttribute(dataIdAttribute);
            const name = button.getAttribute(dataNameAttribute);

            const modalTitle = deleteModal.querySelector(nameTargetId);
            const deleteForm = deleteModal.querySelector(formSelector);

            if (modalTitle) modalTitle.textContent = name;

            const urlTemplate = deleteModal.getAttribute('data-url-template');

            // Replace '0' or 'PLACEHOLDER' with actual ID
            if (urlTemplate && deleteForm) {
                // Handle both conventions if needed, or stick to one. 
                // Currently system seems to use '0' in some places and 'PLACEHOLDER' in others (logic check needed).
                // dashboard.js used 'PLACEHOLDER'. explicit check below.

                let newAction = urlTemplate.replace('PLACEHOLDER', id).replace('/0', '/' + id);
                // Note: simple replace might be risky if ID is '0' (unlikely for GUID). 
                // Better regex: /\/0($|\?)/ -> /id

                // If the template is ending in /0
                if (urlTemplate.endsWith('/0')) {
                    newAction = urlTemplate.substring(0, urlTemplate.length - 1) + id;
                } else {
                    newAction = urlTemplate.replace('PLACEHOLDER', id);
                }

                deleteForm.action = newAction;
            }
        });
    }
}
