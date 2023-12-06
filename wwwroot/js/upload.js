export default {
    data() {
        return {
            selectedFileNames: []
        };
    },
    methods: {
        openFileInput() {
            this.$refs.fileInput.click();
        },
        handleFileChange(event) {
            const fileList = event.target.files;
            const files = [];

            for (let i = 0; i < fileList.length; i++) {
                this.selectedFileNames.push({
                    name: fileList[i].name,
                    size: fileList[i].size
                });
            }
        },
        formatFileSize(size) {
            const units = ["B", "KB", "MB", "GB"];
            let index = 0;

            while (size >= 1024 && index < units.length - 1) {
                size /= 1024;
                index++;
            }

            return `${size.toFixed(2)} ${units[index]}`;
        },
        removeFile(index) {
            this.selectedFileNames.splice(index, 1);
        }
    }
};