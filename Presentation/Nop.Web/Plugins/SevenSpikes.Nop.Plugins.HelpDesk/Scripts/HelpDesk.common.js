function createKendoConfirm(templateId, okCallback, cancelCallback) {

    var kendoWindow = $("<div />").kendoWindow({
        title: "Confirm",
        resizable: false,
        draggable: false,
        modal: true,
        actions: []
    });

    kendoWindow
        .data("kendoWindow")
        .wrapper
        .addClass("confirmation-wrapper");

    kendoWindow
        .data("kendoWindow")
        .content($("#" + templateId).html())
        .center()
        .open();

    kendoWindow
        .find(".confirm, .cancel")
        .click(function (event) {
            event.preventDefault();

            if ($(this).hasClass("confirm")) {

                if (typeof okCallback === "function") {
                    okCallback();
                }
            } else {

                if (typeof cancelCallback === "function") {
                    cancelCallback();
                }
            }

            kendoWindow.data("kendoWindow").close();
        });
}