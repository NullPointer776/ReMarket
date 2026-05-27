(function () {
    function previewFile(file, onLoad) {
        if (!file || !file.type.startsWith('image/')) return;
        const reader = new FileReader();
        reader.onload = function (ev) { onLoad(ev.target.result); };
        reader.readAsDataURL(file);
    }

    function updateGalleryPreview(galleryId, files) {
        if (!window.ItemImageGallery || !files?.length) {
            window.ItemImageGallery?.updatePreview(galleryId, []);
            return;
        }

        const urls = [];
        let pending = 0;

        Array.from(files).forEach(function (file) {
            pending++;
            previewFile(file, function (src) {
                urls.push(src);
                pending--;
                if (pending === 0) {
                    window.ItemImageGallery.updatePreview(galleryId, urls);
                }
            });
        });
    }

    function bindFileInput(inputId, galleryId) {
        const input = document.getElementById(inputId);
        if (!input) return;

        input.addEventListener('change', function (e) {
            const files = e.target.files;
            if (!files?.length) {
                window.ItemImageGallery?.updatePreview(galleryId, []);
                return;
            }
            updateGalleryPreview(galleryId, files);
        });
    }

    document.getElementById('coverImageFile')?.addEventListener('change', function (e) {
        const file = e.target.files?.[0];
        if (!file) return;
        previewFile(file, function (src) {
            window.ItemImageGallery?.updatePreview('upload-preview', [src]);
        });
    });

    bindFileInput('imageFileInput', 'listing-preview');
    bindFileInput('additionalImageInput', 'upload-preview');
})();
