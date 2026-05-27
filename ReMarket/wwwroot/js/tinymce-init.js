(function () {
    if (typeof tinymce === 'undefined') {
        return;
    }

    var textareas = document.querySelectorAll('textarea.rich-text');
    if (!textareas.length) {
        return;
    }

    tinymce.init({
        selector: 'textarea.rich-text',
        plugins: 'lists link autoresize wordcount',
        toolbar: 'undo redo | blocks | bold italic underline strikethrough | alignleft aligncenter alignright | bullist numlist | link removeformat',
        menubar: false,
        height: 320,
        branding: false,
        promotion: false,
        statusbar: true,
        content_style: 'body { font-family: "Plus Jakarta Sans", system-ui, sans-serif; font-size: 14px; }'
    });

    document.querySelectorAll('form').forEach(function (form) {
        form.addEventListener('submit', function () {
            tinymce.triggerSave();
        });
    });
})();
