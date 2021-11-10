import { UserService } from './../shared/user.service';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styles: []
})
export class HomeComponent implements OnInit {
  userDetails;
  dataList:[];
  constructor(private router: Router, private service: UserService) { }

  ngOnInit() {
    let session = sessionStorage.getItem('usermail');

    if (session == null) {
      localStorage.removeItem('token');
      localStorage.removeItem('data1');
      localStorage.removeItem('usermail');
      localStorage.removeItem('userid');
      localStorage.removeItem('username');
      localStorage.removeItem('userfullname');
      this.router.navigate(['/user/login']);
    }
    this.service.getUserProfile().subscribe(
      res => {
        this.userDetails = res;
      },
      err => {
        console.log(err);
      },
    );
    var userid =  localStorage.getItem('userid');
    debugger
    this.service.getAll(userid).subscribe(
      (res: any) => {
        if (res.listData) {
          this.dataList = res.listData;
        } else {
        }
      },
      err => {
        console.log(err);
      }
    );
  }


  onLogout() {
    localStorage.removeItem('token');
    localStorage.removeItem('data1');
    localStorage.removeItem('usermail');
    localStorage.removeItem('userid');
    localStorage.removeItem('username');
    localStorage.removeItem('userfullname');
    this.router.navigate(['/user/login']);
  }
}
