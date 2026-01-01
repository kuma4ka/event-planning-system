import { initDeleteModal } from '../ui/modal-utils.js';

export function initVenueManagement() {
    initDeleteModal('venueDeleteModal', '#venueModalNameDisplay', '#venueDeleteForm', 'data-venue-id', 'data-venue-name');
}