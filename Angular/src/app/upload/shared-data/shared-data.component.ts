import { Component, OnInit } from '@angular/core';
import { UserService } from './../../shared/user.service';

@Component({
  selector: 'app-shared-data',
  templateUrl: './shared-data.component.html',
  styleUrls: ['./shared-data.component.css']
})
export class SharedDataComponent implements OnInit {
  dataList;
  constructor(private serviceuser: UserService) { }

  ngOnInit() {
    var userid =  localStorage.getItem('userid');
    debugger
    this.serviceuser.getAll(userid).subscribe(
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

}
