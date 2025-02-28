window.showModal = (id) => {
    var modal = new bootstrap.Modal(document.getElementById(id));
    modal.show();
};

window.hideModal = (id) => {
    var modalEl = document.getElementById(id);
    var modal = bootstrap.Modal.getInstance(modalEl);
    if (modal) {
        modal.hide();
    }
};
