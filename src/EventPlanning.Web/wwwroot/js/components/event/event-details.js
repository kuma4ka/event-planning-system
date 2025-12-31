export function initEventDetails() {
    if (!document.getElementById('editGuestModal') && !document.getElementById('deleteGuestModal')) return;

    window.openEditGuestModal = (id, firstName, lastName, email, countryCode, phone) => {
        const modalElement = document.getElementById('editGuestModal');
        if (!modalElement) return;

        const idInput = document.getElementById('editGuestId');
        if (idInput) idInput.value = id;

        const fNameInput = document.getElementById('editGuestFirstName');
        if (fNameInput) fNameInput.value = firstName;

        const lNameInput = document.getElementById('editGuestLastName');
        if (lNameInput) lNameInput.value = lastName;

        const emailInput = document.getElementById('editGuestEmail');
        if (emailInput) emailInput.value = email;

        const countryInput = document.getElementById('editGuestCountryCode');
        if (countryInput) countryInput.value = countryCode;

        const phoneInput = document.getElementById('editGuestPhone');
        if (phoneInput) phoneInput.value = phone || '';

        const modal = bootstrap.Modal.getOrCreateInstance(modalElement);
        modal.show();
    };

    window.openDeleteGuestModal = (guestId) => {
        const modalElement = document.getElementById('deleteGuestModal');
        if (!modalElement) return;

        const guestIdInput = document.getElementById('deleteGuestId');
        if (guestIdInput) guestIdInput.value = guestId;

        const modal = bootstrap.Modal.getOrCreateInstance(modalElement);
        modal.show();
    };
}