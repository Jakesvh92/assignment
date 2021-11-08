import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { HttpEventType, HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-upload',
  templateUrl: './upload.component.html',
  styleUrls: ['./upload.component.css']
})

export class UploadComponent implements OnInit {
  public progress: number;
  public message: string;
  public lat;
  public lng;
  public myFiles;
  
  @Output() public onUploadFinished = new EventEmitter();
  constructor(private http: HttpClient) { }
  public ngOnInit(): void {
    this.getLocation();
  }
  onFileChanged(e: any) {
    // this.fileData = <File>fileInput.target.files[0];
    this.myFiles = e.target.files[0];
  }
  getLocation() {
    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition((position: Position) => {
        if (position) {
          // console.log("Latitude: " + position.coords.latitude +
          //   "Longitude: " + position.coords.longitude);
          this.lat = position.coords.latitude;
          this.lng = position.coords.longitude;
          // console.log(this.lat);
          // console.log(this.lat);
        }
      },
        (error: PositionError) => console.log(error));
    } else {
      alert("Geolocation is not supported by this browser.");
    }
  }
  onSubmit(){
    const formData = new FormData();
    var userName =  localStorage. getItem('username');
    var useremail =  localStorage. getItem('usermail');
    var userid =  localStorage. getItem('userid');
debugger;
    formData.append('file', this.myFiles);
    formData.append('latitude', this.lat);
    formData.append('longitude', this.lng);
    formData.append('userName', userName);
    formData.append('useremail', useremail);
    formData.append('userid', userid);
 
    this.http.post('http://localhost:43814/api/ApplicationUser/Upload', formData, {reportProgress: true, observe: 'events'})
      .subscribe(event => {
        if (event.type === HttpEventType.UploadProgress)
          this.progress = Math.round(100 * event.loaded / event.total);
        else if (event.type === HttpEventType.Response) {
          this.message = 'Upload success.';
          this.onUploadFinished.emit(event.body);
        }
      });
  }
}
