function initCropper(config) {
    const input = document.getElementById(config.inputId);
    const imagePreview = document.getElementById(config.previewImageId);
    const croppedImage = document.getElementById(config.croppedImageId);
    const hiddenInput = document.getElementById(config.hiddenFieldId);
    const cancelButton = document.getElementById(config.cancelButtonId);
    const actionWrapper = document.getElementById(config.actionWrapperId);
    const croppedWrapper = document.getElementById(config.croppedWrapperId);

    let cropper;

    input.addEventListener('change', e => {
        const file = e.target.files[0];
        if (!file || !file.type.startsWith("image/")) {
            alert("Будь ласка, виберіть зображення.");
            return;
        }

        if (cropper) {
            cropper.destroy();
            cropper = null;
        }

        imagePreview.src = "";
        imagePreview.style.display = "none";
        croppedImage.src = "";
        croppedImage.style.display = "block";
        croppedWrapper.style.display = "none";
        hiddenInput.value = "";
        actionWrapper.style.display = "none";

        const reader = new FileReader();
        reader.onload = () => {
            imagePreview.src = reader.result;
            imagePreview.style.display = "block";
            actionWrapper.style.display = "flex";

            cropper = new Cropper(imagePreview, {
                aspectRatio: 1,
                viewMode: 1,
                autoCropArea: 1,
                zoomable: false,
                cropend: updateCroppedImage
            });

            setTimeout(() => {
                updateCroppedImage();
            }, 300);
        };
        reader.readAsDataURL(file);
    });

    cancelButton.addEventListener('click', () => {
        if (cropper) {
            cropper.destroy();
            cropper = null;
        }

        imagePreview.src = "";
        imagePreview.style.display = "none";
        croppedImage.src = "";
        croppedImage.style.display = "block";
        croppedWrapper.style.display = "none";
        input.value = "";
        hiddenInput.value = "";
        actionWrapper.style.display = "none";
    });

    function updateCroppedImage() {
        if (!cropper) return;

        const canvas = cropper.getCroppedCanvas({ width: 500, height: 500 });
        if (!canvas) return;

        const base64 = canvas.toDataURL('image/png');
        hiddenInput.value = base64;
        croppedImage.src = base64;
        croppedWrapper.style.display = 'block';
    }
}
