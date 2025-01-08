"use client"
import {useEffect} from "react";
import axios from "axios";

axios.defaults.baseURL = 'http://localhost:5085/api';

export default function Home() {
  useEffect(() => {
    const activate = async function(){
      const response = await axios.get("/Account/Activate/wewe");
      console.log(response);
    }
    activate();
  }, [])
  return (
      <>Hello</>
  );
}
