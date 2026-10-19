(function () {
    function initializeMovieModals() {
        const modalElement = document.getElementById("movieModal");
        const contentElement = document.getElementById("movieModalContent");

        if (!modalElement || !contentElement) {
            return;
        }

        if (typeof bootstrap === "undefined") {
            alert("Bootstrap did not load. Please restart the website.");
            return;
        }

        const movieModal = bootstrap.Modal.getOrCreateInstance(modalElement);

        async function openModal(url) {
            contentElement.innerHTML = `
                <div class="modal-body p-4 text-center">
                    <div class="spinner-border spinner-border-sm"></div>
                    <span class="ms-2">Loading...</span>
                </div>`;

            movieModal.show();

            try {
                const response = await fetch(url);

                if (!response.ok) {
                    throw new Error("Unable to load the movie form.");
                }

                contentElement.innerHTML = await response.text();
            } catch (error) {
                console.error(error);

                contentElement.innerHTML = `
                    <div class="modal-header">
                        <h2 class="modal-title fs-5">Unable to load</h2>
                        <button type="button" class="btn-close"
                                data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        The movie form could not be loaded. Please try again.
                    </div>`;
            }
        }

        async function refreshMovieList() {
            const response = await fetch(window.location.pathname);

            if (!response.ok) {
                throw new Error("Unable to refresh movie list.");
            }

            const html = await response.text();
            const updatedDocument = new DOMParser().parseFromString(html, "text/html");
            const updatedList = updatedDocument.querySelector("#movie-list-container");
            const currentList = document.querySelector("#movie-list-container");

            if (!updatedList || !currentList) {
                throw new Error("Movie list could not be found.");
            }

            currentList.replaceWith(updatedList);
        }

        document.addEventListener("click", function (event) {
            const button = event.target.closest(".js-movie-modal");

            if (!button) {
                return;
            }

            event.preventDefault();

            if (button.dataset.modalUrl) {
                openModal(button.dataset.modalUrl);
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
                submitButton.textContent = "Saving...";
            }

            try {
                const response = await fetch(form.action, {
                    method: "POST",
                    body: new FormData(form),
                    headers: {
                        "X-Requested-With": "XMLHttpRequest"
                    }
                });

                const contentType = response.headers.get("content-type") || "";

                if (!response.ok) {
                    throw new Error("The server returned an error. Check your movie details and try again.");
                }

                if (contentType.includes("application/json")) {
                    const result = await response.json();

                    if (!result.success) {
                        throw new Error("The movie could not be saved.");
                    }

                    movieModal.hide();

                    try {
                        await refreshMovieList();
                    } catch (error) {
                        window.location.reload();
                    }

                    return;
                }

                contentElement.innerHTML = await response.text();
            } catch (error) {
                console.error(error);

                let alertBox = contentElement.querySelector(".modal-request-error");

                if (!alertBox) {
                    alertBox = document.createElement("div");
                    alertBox.className = "alert alert-danger modal-request-error m-3";
                    contentElement.prepend(alertBox);
                }

                alertBox.textContent =
                    error.message || "The movie could not be saved. Please try again.";
            } finally {
                if (submitButton && document.contains(submitButton)) {
                    submitButton.disabled = false;
                    submitButton.textContent = "Save Movie";
                }
            }
        });

        const pageElement = document.querySelector(".movie-index");

        if (pageElement && pageElement.dataset.openCreate === "true") {
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