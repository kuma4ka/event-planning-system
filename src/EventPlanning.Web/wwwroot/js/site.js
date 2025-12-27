import { initDashboard } from './components/dashboard.js';
import { initVenueManagement } from './components/admin/venues.js';
import { initEventDetails } from './components/event/event-details.js';
import {initEventActions} from "./components/event/event-actions.js";

document.addEventListener('DOMContentLoaded', () => {

    initDashboard();
    initVenueManagement();
    initEventDetails();
    initEventActions()

    console.log('Stanza JS loaded modules successfully.');
});