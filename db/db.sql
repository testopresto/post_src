drop database if exists test01;
drop user if exists test01;
create user test01 with encrypted password 'testpass';
create database test01 with owner test01;
grant all privileges on database test01 to test01;
