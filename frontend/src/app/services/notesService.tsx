import api from "@/app/configs/axiosConfig";

function getNotes() {
    return api.get("/Note");
}

export {
    getNotes
};