/* Uppgit 4 och 2 - Unik styling och Ladda upp bilder */
document.addEventListener("DOMContentLoaded", function () {
    let images = document.querySelectorAll(".profileImageMedium, .profileImageLarge, .broadcastImageMedium");

    images.forEach(img => {
        img.addEventListener("click", function () {
            this.style.width = "auto";
            this.style.maxWidth = "100%";
        });
    });
});
