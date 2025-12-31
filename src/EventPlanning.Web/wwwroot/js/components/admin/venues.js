import { initDeleteModal } from '../ui/modal-utils.js';

export function initVenueManagement() {
    initDeleteModal('deleteModal', '#modalVenueName', '#deleteForm', 'data-venue-id', 'data-venue-name');
}