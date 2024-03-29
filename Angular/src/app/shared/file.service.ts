import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class FileService {
  private url = 'http://localhost:43814/api/ApplicationUser';

  constructor(private http: HttpClient) {}

  public upload(formData: FormData) {
    return this.http.post(`${this.url}/upload`, formData, {
      reportProgress: true,
      observe: 'events',
    });
  }

  public download(fileUrl: string) {
    return this.http.get(`${this.url}/download?fileUrl=${fileUrl}`, {
      reportProgress: true,
      observe: 'events',
      responseType: 'blob',
    });
  }

  public getPhotos() {
    return this.http.get(`${this.url}/getPhotos`);
  }
}
