import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SkillTableComponent } from './skill-table.component';

describe('SkillTableComponent', () => {
  let component: SkillTableComponent;
  let fixture: ComponentFixture<SkillTableComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SkillTableComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SkillTableComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
