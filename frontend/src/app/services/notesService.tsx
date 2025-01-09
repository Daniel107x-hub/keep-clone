import api from "@/app/configs/axiosConfig";

function getNotes() {
    return api.get("/Notes");
}

function deleteNote(noteId: number){
    return api.delete(`/Notes/${noteId}`);
}

export {
    getNotes,
    deleteNote
};