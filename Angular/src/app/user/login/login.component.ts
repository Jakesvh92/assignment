import { ToastrService } from 'ngx-toastr';
import { UserService } from './../../shared/user.service';
import { Component, OnInit } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styles: []
})
export class LoginComponent implements OnInit {
  formModel = {
    UserName: '',
    Password: ''
  }
  listData:[];
  user = {
    fullName:"",
    id:"",
    userName:"",
    normalizedUserName:"",
    email:"",
    normalizedEmail:"",
    emailConfirmed:false,
    passwordHash:"",
    securityStamp:"",
    concurrencyStamp:"",
    phoneNumber:null,
    phoneNumberConfirmed:false,
    twoFactorEnabled:false,
    lockoutEnd:null,
    lockoutEnabled:true,
    accessFailedCount:0
 }
  constructor(private service: UserService, private router: Router, private toastr: ToastrService) { }

  ngOnInit() {
    if (localStorage.getItem('token') != null)
      this.router.navigateByUrl('/home');
  }

  onSubmit(form: NgForm) {
    this.service.login(form.value).subscribe(
      (res: any) => {
        debugger;
        var user = this.user;
        user = res.user;
        this.listData = res.listData;
        localStorage.setItem('token', res.token);
        localStorage.setItem('usermail', user.email);
        localStorage.setItem('userid', user.id);
        localStorage.setItem('username', user.userName);
        localStorage.setItem('userfullname', user.fullName);
        this.router.navigateByUrl('/home');
      },
      err => {
        if (err.status == 400)
          this.toastr.error('Incorrect username or password.', 'Authentication failed.');
        else
          console.log(err);
      }
    );
  }
}
