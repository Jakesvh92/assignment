import { Component, OnInit, Output, Input, EventEmitter } from '@angular/core';
import { HttpEventType, HttpClient, HttpResponse } from '@angular/common/http';
import { FormBuilder, Validators } from '@angular/forms';
import { FileService } from './../shared/file.service';
import { UserService } from './../shared/user.service';
import { ToastrService } from 'ngx-toastr';

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
  public resData: any;
  
  public tags: string = '';
  public imgtype: string = '';
  dataList: [];
  UserList: [];
  @Output() public onUploadFinished = new EventEmitter();
  @Input() public fileUrl: string;
  constructor(private http: HttpClient, private formBuilder: FormBuilder, private fileService: FileService,
    private serviceuser: UserService, private toastr: ToastrService) { }
  
  formModel = this.formBuilder.group({
    tags: ['', Validators.required],
    imgtype: ['', Validators.required]
  });
  formModel1 = this.formBuilder.group({
    ShareWith: ['', Validators.required]
  });

  public ngOnInit(): void {
    this.getLocation();
    var userid =  localStorage.getItem('userid');
    this.serviceuser.getAll(userid).subscribe(
      (res: any) => {
        if (res.succeeded) {
          debugger
          this.dataList = res.body;
        } else {
          
        }
      },
      err => {
        console.log(err);
      }
    );
  }
  onFileChanged(e: any) {
    debugger
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
    var userName =  localStorage.getItem('username');
    var useremail =  localStorage.getItem('usermail');
    var userid =  localStorage.getItem('userid');


    formData.append('file', this.myFiles);
    formData.append('latitude', this.lat);
    formData.append('longitude', this.lng);
    formData.append('userName', userName);
    formData.append('useremail', useremail);
    formData.append('userid', userid);
    formData.append('tags', this.formModel.value.tags);
    formData.append('imgtype', this.formModel.value.imgtype);
    debugger
    this.http.post('http://localhost:43814/api/ApplicationUser/Upload', formData, {reportProgress: true, observe: 'events'})
      .subscribe(event => {
        if (event.type === HttpEventType.UploadProgress)
          this.progress = Math.round(100 * event.loaded / event.total);
        else if (event.type === HttpEventType.Response) {
          debugger
          this.message = 'Upload success.';
          this.resData = event.body;
          // this.onUploadFinished.emit(this.dataList);
        }
      });
  }
  download(file) {
    this.fileService.download(file).subscribe((event) => {
      this.fileUrl = "http://localhost:43814/resources/images/"+file;
      if (event.type === HttpEventType.UploadProgress)
        this.progress = Math.round((100 * event.loaded) / event.total);
      else if (event.type === HttpEventType.Response) {
        this.message = 'Download success.';
        this.downloadFile(event);
      }
    });
  }

  private downloadFile(data: HttpResponse<Blob>) {
    const downloadedFile = new Blob([data.body], { type: data.body.type });
    const a = document.createElement('a');
    a.setAttribute('style', 'display:none;');
    document.body.appendChild(a);
    a.download = this.fileUrl;
    a.href = URL.createObjectURL(downloadedFile);
    a.target = '_blank';
    a.click();
    document.body.removeChild(a);
  }

  deleteRow(Id) {
    this.serviceuser.delete(Id).subscribe(
      (res: any) => {
        if (res.succeeded) {
          this.toastr.success('Image deleted !! Successfully');
        } else {
          res.errors.forEach(element => {
            
          });
        }
      },
      err => {
        console.log(err);
      }
    );
  }
  onSubmitShare(Id){
    const formData = new FormData();
    var userid =  localStorage.getItem('userid');
    formData.append('userid', userid);
    formData.append('imgId', Id);
    formData.append('sharedTo', this.formModel.value.ShareWith);
    this.http.post('http://localhost:43814/api/ApplicationUser/SaveSharedImage', formData, {reportProgress: true, observe: 'events'})
      .subscribe(event => {
        debugger;
        if (event.type === HttpEventType.UploadProgress)
          this.progress = Math.round(100 * event.loaded / event.total);
        else if (event.type === HttpEventType.Response) {
          this.message = 'save success.';
          this.resData = event.body;
          // this.onUploadFinished.emit(this.dataList);
        }
      });
  }
}
