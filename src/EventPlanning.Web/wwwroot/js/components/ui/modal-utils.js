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

            if (urlTemplate && deleteForm) {
                let newAction = urlTemplate.replace('PLACEHOLDER', id).replace('/0', '/' + id);

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
