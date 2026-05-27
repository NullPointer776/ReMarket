(function () {
    var AUTO_INTERVAL_MS = 5000;

    function getThumbs(gallery) {
        return gallery.querySelectorAll('.item-image-gallery__thumb[data-src]');
    }

    function getTrack(gallery) {
        return gallery.querySelector('.item-image-gallery__track');
    }

    function getViewport(gallery) {
        return gallery.querySelector('.item-image-gallery__viewport');
    }

    function getSlides(gallery) {
        var track = getTrack(gallery);
        return track ? track.querySelectorAll('.item-image-gallery__slide') : [];
    }

    function getCurrentIndex(gallery) {
        var active = gallery.querySelector('.item-image-gallery__thumb.active[data-src]');
        if (active) return parseInt(active.dataset.index, 10);
        return parseInt(gallery.dataset.currentIndex || '0', 10);
    }

    function syncSlideWidths(gallery) {
        var viewport = getViewport(gallery);
        var track = getTrack(gallery);
        if (!viewport || !track) return 0;

        var width = viewport.clientWidth;
        if (width <= 0) return 0;

        var slides = getSlides(gallery);
        slides.forEach(function (slide) {
            slide.style.width = width + 'px';
            slide.style.maxWidth = width + 'px';
            slide.style.flexShrink = '0';
        });

        track.style.width = (width * slides.length) + 'px';
        return width;
    }

    function stopAutoplay(gallery) {
        if (gallery._autoplayTimer) {
            clearInterval(gallery._autoplayTimer);
            gallery._autoplayTimer = null;
        }
    }

    function startAutoplay(gallery) {
        stopAutoplay(gallery);
        if (gallery.dataset.autoplayEnabled !== 'true') return;
        if (gallery.dataset.autoplayPaused === 'true') return;
        if (getThumbs(gallery).length <= 1) return;

        gallery._autoplayTimer = setInterval(function () {
            setActiveThumb(gallery, getCurrentIndex(gallery) + 1, false);
        }, AUTO_INTERVAL_MS);
    }

    function resetAutoplay(gallery) {
        stopAutoplay(gallery);
        startAutoplay(gallery);
    }

    function moveTrack(gallery, index, animate) {
        var track = getTrack(gallery);
        if (!track) return;

        var slideWidth = syncSlideWidths(gallery);
        if (!slideWidth) {
            slideWidth = getViewport(gallery)?.clientWidth || 0;
        }
        if (slideWidth <= 0) return;

        if (animate === false) {
            track.classList.add('item-image-gallery__track--instant');
        }

        track.style.transform = 'translate3d(-' + (index * slideWidth) + 'px, 0, 0)';

        if (animate === false) {
            requestAnimationFrame(function () {
                track.classList.remove('item-image-gallery__track--instant');
            });
        }
    }

    function bindGalleryLayout(gallery) {
        if (gallery._layoutBound) return;
        gallery._layoutBound = true;

        var viewport = getViewport(gallery);
        if (!viewport) return;

        var relayout = function () {
            var index = getCurrentIndex(gallery);
            moveTrack(gallery, index, false);
        };

        if (typeof ResizeObserver !== 'undefined') {
            gallery._resizeObserver = new ResizeObserver(relayout);
            gallery._resizeObserver.observe(viewport);
        }

        window.addEventListener('resize', relayout);

        if (document.fonts && document.fonts.ready) {
            document.fonts.ready.then(relayout);
        }

        gallery.querySelectorAll('.item-image-gallery__main').forEach(function (img) {
            if (!img.complete) {
                img.addEventListener('load', relayout, { once: true });
            }
        });
    }

    function setActiveThumb(gallery, index, userInitiated) {
        var thumbs = getThumbs(gallery);
        if (!thumbs.length) return;

        var count = thumbs.length;
        var current = getCurrentIndex(gallery);
        index = ((index % count) + count) % count;

        if (index === current && gallery.dataset.currentIndex !== undefined) {
            if (userInitiated) resetAutoplay(gallery);
            return;
        }

        gallery.dataset.currentIndex = String(index);

        thumbs.forEach(function (thumb) {
            var isActive = parseInt(thumb.dataset.index, 10) === index;
            thumb.classList.toggle('active', isActive);
            thumb.setAttribute('aria-selected', isActive ? 'true' : 'false');
        });

        moveTrack(gallery, index, true);

        if (userInitiated) resetAutoplay(gallery);
    }

    function buildStageHtml(list, altText) {
        if (list.length === 0) {
            return '<div class="item-image-gallery__empty w-100 h-100 d-flex flex-column align-items-center justify-content-center text-muted">' +
                '<i class="bi bi-images fs-3 mb-1"></i><span class="small">No images yet</span></div>';
        }

        var slides = list.map(function (src) {
            return '<a href="' + src + '" class="item-image-gallery__slide item-image-gallery__main-link" target="_blank" rel="noopener" title="Open image">' +
                '<img src="' + src + '" alt="' + (altText || 'Preview') + '" class="item-image-gallery__main w-100 h-100" style="object-fit: cover;" /></a>';
        }).join('');

        var navHtml = list.length > 1
            ? '<button type="button" class="item-image-gallery__nav item-image-gallery__nav--prev" aria-label="Previous image">' +
              '<i class="bi bi-chevron-left"></i></button>' +
              '<button type="button" class="item-image-gallery__nav item-image-gallery__nav--next" aria-label="Next image">' +
              '<i class="bi bi-chevron-right"></i></button>'
            : '';

        return '<div class="item-image-gallery__viewport">' +
            '<div class="item-image-gallery__track">' + slides + '</div></div>' + navHtml;
    }

    function initGallery(gallery) {
        if (gallery.dataset.galleryInit === 'true') return;

        var thumbs = getThumbs(gallery);
        gallery.dataset.galleryInit = 'true';
        gallery.dataset.autoplayEnabled = thumbs.length > 1 ? 'true' : 'false';

        var startIndex = getCurrentIndex(gallery);
        gallery.dataset.currentIndex = String(startIndex);

        bindGalleryLayout(gallery);
        moveTrack(gallery, startIndex, false);

        gallery.addEventListener('click', function (e) {
            if (e.target.closest('.item-image-gallery__delete')) return;

            var prev = e.target.closest('.item-image-gallery__nav--prev');
            if (prev && gallery.contains(prev)) {
                e.preventDefault();
                setActiveThumb(gallery, getCurrentIndex(gallery) - 1, true);
                return;
            }

            var next = e.target.closest('.item-image-gallery__nav--next');
            if (next && gallery.contains(next)) {
                e.preventDefault();
                setActiveThumb(gallery, getCurrentIndex(gallery) + 1, true);
                return;
            }

            var thumb = e.target.closest('.item-image-gallery__thumb[data-src]');
            if (!thumb || !gallery.contains(thumb)) return;
            e.preventDefault();
            setActiveThumb(gallery, parseInt(thumb.dataset.index, 10), true);
        });

        gallery.addEventListener('mouseenter', function () {
            gallery.dataset.autoplayPaused = 'true';
            stopAutoplay(gallery);
        });

        gallery.addEventListener('mouseleave', function () {
            gallery.dataset.autoplayPaused = 'false';
            startAutoplay(gallery);
        });

        gallery.addEventListener('focusin', function () {
            gallery.dataset.autoplayPaused = 'true';
            stopAutoplay(gallery);
        });

        gallery.addEventListener('focusout', function () {
            if (!gallery.contains(document.activeElement)) {
                gallery.dataset.autoplayPaused = 'false';
                startAutoplay(gallery);
            }
        });

        startAutoplay(gallery);
    }

    window.ItemImageGallery = {
        initAll: function () {
            document.querySelectorAll('[data-item-image-gallery]').forEach(function (gallery) {
                gallery.dataset.galleryInit = 'false';
                initGallery(gallery);
            });
        },
        updatePreview: function (galleryId, urls) {
            var gallery = document.getElementById('gallery-' + galleryId);
            if (!gallery) return;

            stopAutoplay(gallery);

            var list = (urls || []).filter(Boolean);
            var stage = gallery.querySelector('.item-image-gallery__stage');
            var thumbs = gallery.querySelector('.item-image-gallery__thumbs');
            if (!stage || !thumbs) return;

            gallery.dataset.galleryInit = 'false';
            gallery._layoutBound = false;
            if (gallery._resizeObserver) {
                gallery._resizeObserver.disconnect();
                gallery._resizeObserver = null;
            }

            thumbs.innerHTML = '';

            if (list.length === 0) {
                stage.innerHTML = buildStageHtml([], 'Preview');
                for (var p = 0; p < 4; p++) {
                    thumbs.insertAdjacentHTML(
                        'beforeend',
                        '<div class="item-image-gallery__thumb item-image-gallery__thumb--placeholder border rounded bg-light" aria-hidden="true"></div>'
                    );
                }
                gallery.dataset.autoplayEnabled = 'false';
                initGallery(gallery);
                return;
            }

            stage.innerHTML = buildStageHtml(list, 'Preview');

            list.forEach(function (src, index) {
                thumbs.insertAdjacentHTML(
                    'beforeend',
                    '<div class="item-image-gallery__thumb-wrap position-relative">' +
                    '<button type="button" class="item-image-gallery__thumb border rounded overflow-hidden p-0' +
                    (index === 0 ? ' active' : '') +
                    '" data-index="' + index + '" data-src="' + src + '" aria-label="Show image ' + (index + 1) + '" aria-selected="' +
                    (index === 0 ? 'true' : 'false') + '">' +
                    '<img src="' + src + '" alt="" class="w-100 h-100" style="object-fit: cover;" /></button></div>'
                );
            });

            gallery.dataset.currentIndex = '0';
            initGallery(gallery);
        }
    };

    document.addEventListener('DOMContentLoaded', function () {
        window.ItemImageGallery.initAll();
    });
})();
