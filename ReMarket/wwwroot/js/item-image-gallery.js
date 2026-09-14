(function () {
    'use strict';

    const AUTO_INTERVAL_MS = 5000;

    function escapeHtml(value) {
        return String(value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

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
        const track = getTrack(gallery);
        return track ? track.querySelectorAll('.item-image-gallery__slide') : [];
    }

    function getCurrentIndex(gallery) {
        const active = gallery.querySelector('.item-image-gallery__thumb.active[data-src]');
        if (active) return parseInt(active.dataset.index, 10);
        return parseInt(gallery.dataset.currentIndex || '0', 10);
    }

    function syncSlideWidths(gallery) {
        const viewport = getViewport(gallery);
        const track = getTrack(gallery);
        if (!viewport || !track) return 0;

        const width = viewport.clientWidth;
        if (width <= 0) return 0;

        const slides = getSlides(gallery);
        slides.forEach((slide) => {
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

        gallery._autoplayTimer = setInterval(() => {
            setActiveThumb(gallery, getCurrentIndex(gallery) + 1, false);
        }, AUTO_INTERVAL_MS);
    }

    function resetAutoplay(gallery) {
        stopAutoplay(gallery);
        startAutoplay(gallery);
    }

    function moveTrack(gallery, index, animate) {
        const track = getTrack(gallery);
        if (!track) return;

        let slideWidth = syncSlideWidths(gallery);
        if (!slideWidth) {
            slideWidth = getViewport(gallery)?.clientWidth || 0;
        }
        if (slideWidth <= 0) return;

        if (animate === false) {
            track.classList.add('item-image-gallery__track--instant');
        }

        track.style.transform = 'translate3d(-' + (index * slideWidth) + 'px, 0, 0)';

        if (animate === false) {
            requestAnimationFrame(() => {
                track.classList.remove('item-image-gallery__track--instant');
            });
        }
    }

    function bindGalleryLayout(gallery) {
        if (gallery._layoutBound) return;
        gallery._layoutBound = true;

        const viewport = getViewport(gallery);
        if (!viewport) return;

        const relayout = () => {
            const index = getCurrentIndex(gallery);
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

        gallery.querySelectorAll('.item-image-gallery__main').forEach((img) => {
            if (!img.complete) {
                img.addEventListener('load', relayout, { once: true });
            }
        });
    }

    function setActiveThumb(gallery, index, userInitiated) {
        const thumbs = getThumbs(gallery);
        if (!thumbs.length) return;

        const count = thumbs.length;
        const current = getCurrentIndex(gallery);
        index = ((index % count) + count) % count;

        if (index === current && gallery.dataset.currentIndex !== undefined) {
            if (userInitiated) resetAutoplay(gallery);
            return;
        }

        gallery.dataset.currentIndex = String(index);

        thumbs.forEach((thumb) => {
            const isActive = parseInt(thumb.dataset.index, 10) === index;
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

        const slides = list.map((src) => {
            return '<a href="' + escapeHtml(src) + '" class="item-image-gallery__slide item-image-gallery__main-link" target="_blank" rel="noopener" title="Open image">' +
                '<img src="' + escapeHtml(src) + '" alt="' + escapeHtml(altText || 'Preview') + '" class="item-image-gallery__main w-100 h-100" style="object-fit: cover;" /></a>';
        }).join('');

        const navHtml = list.length > 1
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

        const thumbs = getThumbs(gallery);
        gallery.dataset.galleryInit = 'true';
        gallery.dataset.autoplayEnabled = thumbs.length > 1 ? 'true' : 'false';

        const startIndex = getCurrentIndex(gallery);
        gallery.dataset.currentIndex = String(startIndex);

        bindGalleryLayout(gallery);
        moveTrack(gallery, startIndex, false);

        gallery.addEventListener('click', (e) => {
            if (e.target.closest('.item-image-gallery__delete')) return;

            const prev = e.target.closest('.item-image-gallery__nav--prev');
            if (prev && gallery.contains(prev)) {
                e.preventDefault();
                setActiveThumb(gallery, getCurrentIndex(gallery) - 1, true);
                return;
            }

            const next = e.target.closest('.item-image-gallery__nav--next');
            if (next && gallery.contains(next)) {
                e.preventDefault();
                setActiveThumb(gallery, getCurrentIndex(gallery) + 1, true);
                return;
            }

            const thumb = e.target.closest('.item-image-gallery__thumb[data-src]');
            if (!thumb || !gallery.contains(thumb)) return;
            e.preventDefault();
            setActiveThumb(gallery, parseInt(thumb.dataset.index, 10), true);
        });

        gallery.addEventListener('mouseenter', () => {
            gallery.dataset.autoplayPaused = 'true';
            stopAutoplay(gallery);
        });

        gallery.addEventListener('mouseleave', () => {
            gallery.dataset.autoplayPaused = 'false';
            startAutoplay(gallery);
        });

        gallery.addEventListener('focusin', () => {
            gallery.dataset.autoplayPaused = 'true';
            stopAutoplay(gallery);
        });

        gallery.addEventListener('focusout', () => {
            if (!gallery.contains(document.activeElement)) {
                gallery.dataset.autoplayPaused = 'false';
                startAutoplay(gallery);
            }
        });

        startAutoplay(gallery);
    }

    window.ItemImageGallery = {
        initAll: function () {
            document.querySelectorAll('[data-item-image-gallery]').forEach((gallery) => {
                gallery.dataset.galleryInit = 'false';
                initGallery(gallery);
            });
        },
        updatePreview: function (galleryId, urls) {
            const gallery = document.getElementById('gallery-' + galleryId);
            if (!gallery) return;

            stopAutoplay(gallery);

            const list = (urls || []).filter(Boolean);
            const stage = gallery.querySelector('.item-image-gallery__stage');
            const thumbs = gallery.querySelector('.item-image-gallery__thumbs');
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
                for (let p = 0; p < 4; p++) {
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

            list.forEach((src, index) => {
                thumbs.insertAdjacentHTML(
                    'beforeend',
                    '<div class="item-image-gallery__thumb-wrap position-relative">' +
                    '<button type="button" class="item-image-gallery__thumb border rounded overflow-hidden p-0' +
                    (index === 0 ? ' active' : '') +
                    '" data-index="' + index + '" data-src="' + escapeHtml(src) + '" aria-label="Show image ' + (index + 1) + '" aria-selected="' +
                    (index === 0 ? 'true' : 'false') + '">' +
                    '<img src="' + escapeHtml(src) + '" alt="" class="w-100 h-100" style="object-fit: cover;" /></button></div>'
                );
            });

            gallery.dataset.currentIndex = '0';
            initGallery(gallery);
        }
    };

    document.addEventListener('DOMContentLoaded', () => {
        window.ItemImageGallery.initAll();
    });
})();
