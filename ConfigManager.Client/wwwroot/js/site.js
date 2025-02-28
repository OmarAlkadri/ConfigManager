window.showModal = (modalId) => {
    if (typeof bootstrap !== "undefined") {
        var modalElement = document.getElementById(modalId);
        if (modalElement) {
            var modal = new bootstrap.Modal(modalElement);
            modal.show();
        } else {
            console.error(`Modal with ID '${modalId}' not found.`);
        }
    } else {
        console.error("Bootstrap is not loaded. Please check your imports.");
    }
};

window.hideModal = (modalId) => {
    if (typeof bootstrap !== "undefined") {
        var modalElement = document.getElementById(modalId);
        if (modalElement) {
            var modal = bootstrap.Modal.getInstance(modalElement);
            if (modal) {
                modal.hide();
            } else {
                console.error(`No Bootstrap modal instance found for ID '${modalId}'.`);
            }
        } else {
            console.error(`Modal with ID '${modalId}' not found.`);
        }
    } else {
        console.error("Bootstrap is not loaded. Please check your imports.");
    }
};
