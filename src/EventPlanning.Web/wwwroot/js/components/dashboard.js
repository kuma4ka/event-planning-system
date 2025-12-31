import { initDeleteModal } from './ui/modal-utils.js';

export function initDashboard() {
    initDeleteModal('deleteModal', '#modalEventName', '#deleteForm', 'data-event-id', 'data-event-name');
}