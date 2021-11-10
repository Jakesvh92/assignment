import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SharedDatawithmeComponent } from './shared-datawithme.component';

describe('SharedDatawithmeComponent', () => {
  let component: SharedDatawithmeComponent;
  let fixture: ComponentFixture<SharedDatawithmeComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SharedDatawithmeComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SharedDatawithmeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
