(function () {
    function initializeMovieModals() {
        const modalElement = document.getElementById("movieModal");
        const contentElement = document.getElementById("movieModalContent");

        if (!modalElement || !contentElement) {
            return;
        }

        if (typeof bootstrap === "undefined") {
            console.error("Bootstrap JavaScript is not loaded.");
            return;
        }

        const movieModal = bootstrap.Modal.getOrCreateInstance(modalElement);

        async function openModal(url) {
            contentElement.innerHTML = `
                <div class="modal-body p-4 text-center">
                    <div class="spinner-border spinner-border-sm text-primary" role="status"></div>
                    <span class="ms-2">Loading...</span>
                </div>`;

            movieModal.show();

            try {
                const response = await fetch(url, {
                    headers: {
                        "X-Requested-With": "XMLHttpRequest"
                    }
                });

                if (!response.ok) {
                    throw new Error("Unable to load the popup.");
                }

                contentElement.innerHTML = await response.text();
            } catch (error) {
                console.error(error);

                contentElement.innerHTML = `
                    <div class="modal-header">
                        <h2 class="modal-title fs-5">Unable to load</h2>
                        <button type="button" class="btn-close"
                                data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        The requested information could not be loaded.
                    </div>`;
            }
        }

        async function refreshMovieList() {
            const response = await fetch(window.location.pathname, {
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                }
            });

            if (!response.ok) {
                throw new Error("Unable to refresh the movie list.");
            }

            const html = await response.text();
            const documentFromResponse = new DOMParser().parseFromString(html, "text/html");

            const updatedList = documentFromResponse.querySelector("#movie-list-container");
            const currentList = document.querySelector("#movie-list-container");

            if (!updatedList || !currentList) {
                throw new Error("Movie list container was not found.");
            }

            currentList.replaceWith(updatedList);
        }

        document.addEventListener("click", function (event) {
            const button = event.target.closest(".js-movie-modal");

            if (!button) {
                return;
            }

            event.preventDefault();

            const url = button.dataset.modalUrl;

            if (url) {
                openModal(url);
            }
        });

        document.addEventListener("submit", async function (event) {
            const form = event.target.closest("#movieModal form.js-modal-form");

            if (!form) {
                return;
            }

            event.preventDefault();

            const submitButton = form.querySelector('button[type="submit"]');

            if (submitButton) {
                submitButton.disabled = true;
            }

            try {
                const response = await fetch(form.action, {
                    method: (form.method || "POST").toUpperCase(),
                    body: new FormData(form),
                    headers: {
                        "X-Requested-With": "XMLHttpRequest"
                    }
                });

                const contentType = response.headers.get("content-type") || "";

                if (!response.ok) {
                    throw new Error("The server could not process the request.");
                }

                if (contentType.includes("application/json")) {
                    const result = await response.json();

                    if (!result.success) {
                        throw new Error("The operation was not completed.");
                    }

                    movieModal.hide();

                    try {
                        await refreshMovieList();
                    } catch (refreshError) {
                        console.error(refreshError);
                        window.location.reload();
                    }

                    return;
                }

                contentElement.innerHTML = await response.text();
            } catch (error) {
                console.error(error);

                let alert = contentElement.querySelector(".modal-request-error");

                if (!alert) {
                    alert = document.createElement("div");
                    alert.className = "alert alert-danger modal-request-error m-3";
                    contentElement.prepend(alert);
                }

                alert.textContent = "The request failed. Please try again.";
            } finally {
                if (submitButton && document.contains(submitButton)) {
                    submitButton.disabled = false;
                }
            }
        });

        const pageElement = document.querySelector(".movie-index");

        if (pageElement?.dataset.openCreate === "true") {
            const url = new URL(window.location.href);
            url.searchParams.delete("openCreate");

            window.history.replaceState(
                {},
                "",
                url.pathname + url.search + url.hash
            );

            const addButton = document.querySelector(
                '.js-movie-modal[data-modal-type="create"]'
            );

            if (addButton) {
                addButton.click();
            }
        }
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initializeMovieModals);
    } else {
        initializeMovieModals();
    }
})();